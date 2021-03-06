require 'rubygems'
require 'albacore'
require 'rake/clean'

VERSION = "0.1.0"
OUTPUT = "build"
CONFIGURATION = 'Release'
SHARED_ASSEMBLY_INFO = 'src/SharedAssemblyInfo.cs'
SOLUTION_FILE = 'src/Aqueduct.Appia.sln'

Albacore.configure do |config|
	config.log_level = :verbose
	config.msbuild.use :net4
end

desc "Compiles solution and runs unit tests"
task :default => [:clean, :version, :compile, :aspnet, :host]

#Add the folders that should be cleaned as part of the clean task
CLEAN.include(OUTPUT)
CLEAN.include(FileList["src/**/#{CONFIGURATION}"])

desc "Update shared assemblyinfo file for the build"
assemblyinfo :version => [:clean] do |asm|
	asm.version = VERSION
	asm.company_name = "Aqueduct"
	asm.product_name = "Appia"
	asm.description = "Appia is a front end templating framework based on Nancy and using Razor as a view engine"
	asm.copyright = "Copyright (C) Aqueduct"
	asm.output_file = SHARED_ASSEMBLY_INFO
end

desc "Compile solution file"
msbuild :compile => [:version] do |msb|
	msb.properties :configuration => CONFIGURATION
	msb.targets :Clean, :Build
	msb.solution = SOLUTION_FILE
end

desc "Gathers output files and copies them to the output folder"
task :aspnet => [:compile] do
	Dir.mkdir(OUTPUT)
	Dir.mkdir("#{OUTPUT}/aspnet")

	FileUtils.cp_r FileList["src/Aqueduct.Appia.AspNet/**"].exclude(/obj/).exclude(/Properties/).exclude(/.*csproj.*/), "#{OUTPUT}/aspnet"
	
	Rake::Task["aspnet_merge"].execute
	
	FileList['build/aspnet/bin/*.*'].exclude(/Aqueduct\.Appia\.dll/).exclude(/Aqueduct\.Appia\.pdb/).each { |file| File.delete(file) }
end

desc "Merges the aspnet dll into a singe one dll"
exec :aspnet_merge => [:aspnet] do |cmd|
    cmd.command = 'tools\ilmerge\ilmerge.exe'
    cmd.parameters = '/out:build\aspnet\bin\Aqueduct.Appia.dll ' + FileList['build/aspnet/bin/*.dll'].join(' ') + ' /targetplatform:v4,C:\Windows\Microsoft.NET\Framework\v4.0.30319'
end

task :test do |cmd| 
	puts FileList["#{OUTPUT}/aspnet/bin/*.*"].join(' ')
end

desc "Gathers output files and copies them to the output folder"
task :host => [:compile] do
	Dir.mkdir("#{OUTPUT}/host")

	FileUtils.cp_r FileList["src/Aqueduct.Appia.Host/bin/#{CONFIGURATION}/*.*"], "#{OUTPUT}/host"
	FileUtils.cp_r FileList["src/Aqueduct.Appia.AspNet/**"].exclude(/obj/).exclude(/bin/).exclude(/Properties/).exclude(/.*csproj.*/).exclude(/.*config$/), "#{OUTPUT}/host"
	
	Rake::Task["host_merge"].execute
	
	FileList['build/host/*.*'].exclude(/appia\.(exe|pdb)/).each { |file| File.delete(file) }
	
	#Create the start.bat file
	File.open( "build/host/start.bat", "w" ) do |file|
		file << "appia.exe -a localhost -p 8888"
	end
end

desc "Merges the standalone assemblies to a single dll"
exec :host_merge do |cmd|
    cmd.command = 'tools\ilmerge\ilmerge.exe'
    cmd.parameters = '/out:build\host\appia.exe build\host\Aqueduct.Appia.Host.exe ' + FileList['build/host/*.dll'].join(' ') + ' /targetplatform:v4,C:\Windows\Microsoft.NET\Framework\v4.0.30319'
    puts 'merged.'
end
