Feature: Export as html
	As a Front end developer
	I want to create a flat representation of my work
	So I can submit it to the client or send it to my fellow backend developers
	
@init
Scenario: Create export folder
	Given I have a export path
	When I initialise the exporter
	Then an export folder is created

@init
Scenario: Delete export foldre if already exists
	Given I have an export folder
	And I have an export path pointing to the same export folder
	When I initialise the exporter
	Then a new empty export folder is created in place of the old one

@export
Scenario: Export pages
	Given I have page.cshtml in the pages folder
	When I export the site
	Then I have page.html in the export folder

@export
Scenario: Export all pages
	Given I have 4 pages
	When I export
	Then I have 4 html files in the export directory

@export
Scenario: Export layouts
	Given I have page "page_with_layout.cshtml"
	And it uses layout "layout.cshtml"
	When I export
	Then I have html page "page_with_layout.html"
	And "page_with_layout.html" contains html content from layout "layout.cshtml"
	And "page_with_layout.html" contains html content from page "page_with_layout.cshtml"

@export
Scenario: Export partials
	Given I have page "page_with_partial.cshtml"
	And it uses partial "partial.cshtml"
	When I export
	Then I have html page "page_with_partial.html"
	And "page_with_partial.html" contains html content from partial "partial.cshtml"
	And "page_with_partial.html" contains html content from page "page_with_partial.cshtml"


@export
Scenario: Copy static files
	Given I have css folder with 3 files
	And I have js folder with 3 files
	When I export
	Then I have css folder with 3 files in the export folder
	And I have js folder with 3 files in the export folder

@export
Scenario: Copy static file from root
	Given I have static file "static.txt"
	When I export
	Then I have file "static.txt" in the export folder





