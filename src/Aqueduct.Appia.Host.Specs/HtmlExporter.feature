Feature: Export as html
	As a Front end developer
	I want to create a flat representation of my work
	So I can submit it to the client or send it to my fellow backend developers
	
@init
Scenario: Create export folder
	Given a new HtmlExporter
	When I initialise the exporter
	Then an export folder is created

@init
Scenario: Delete export foldre if already exists
	Given a new HtmlExporter
	And I have an export folder already containing 2 files
	When I initialise the exporter
	Then a new empty export folder is created in place of the old one

@export
Scenario: Export all pages
	Given a new HtmlExporter
	And I have 4 cshtml files in the pages folder
	When I export
	Then I have 4 html files in the export directory

@export
Scenario: Export layouts
	Given a new HtmlExporter
	And I have page "page_with_layout.cshtml"
	And it uses layout "layout.cshtml"
	When I export
	Then I have html page "page_with_layout.html"
	And "page_with_layout.html" contains html content from layout "layout.cshtml"
	And "page_with_layout.html" contains html content from page "page_with_layout.cshtml"

@export
Scenario: Export partials
	Given a new HtmlExporter
	And I have page "page_with_partial.cshtml"
	And it uses partial "partial.cshtml"
	When I export
	Then "page_with_partial.html" contains html content from partial "partial.cshtml"
	And "page_with_partial.html" contains html content from page "page_with_partial.cshtml"


@export
Scenario: Copy static files
	Given a new HtmlExporter
	And I have css folder with 3 files
	And I have js folder with 3 files
	When I export
	Then I have css folder with 3 files in the export folder
	And I have js folder with 3 files in the export folder

@export
Scenario: Copy static file from root
	Given a new HtmlExporter
	And I have static file "static.txt"
	When I export
	Then I have file "static.txt" in the export folder





