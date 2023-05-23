/*! \mainpage CAD Tools Documentation
\section intro_sec Introduction
The purpose of CADTools is to provide an AutoCAD Tool Palette to assust in the management of CAD Standards to suit various clients differing requirements.
The design of the application was modelled on the [Queensland Transport AutoCAD Customisation](https://www.tmr.qld.gov.au/business-industry/Road-systems-and-engineering/Software/Transport-and-Main-Roads-AutoCAD-customisation) and uses similar formated customisation files. Not all of these customisations have been implemented yet. 
The application contains 4 tabs...

\subsection intro_sec_1 Templates
The Templates section is a file tree constructed from the contents of a folder on the Company file system.  
The application searched recursivly from the root folder for all AutoCAD .dwt files and lists them in a file tree.
The user can double click on one of these templates to open a new drawing using the respective template.
\subsection intro_sec_2 Blocks
The Blocks section is a file tree constructed from the contents of a folder on the Company file system.
The application searched recursivly from the root folder for all AutoCAD .dwg files and lists them in a file tree.
At present the application does not searh inside these foles for blocks contained there but it is the intention to add this functionality.
At present the application does not have a customisation file to set default layers and space for the insertion of th block (as the TMR customisation does) but it is the intention to add this functionality.

\subsection intro_sec_3 Layers
The Layers section is a file tree which reads an xml file from the Company file system which is in the same format as the TMR customisation.  
This file can group layers according to client and respective sub branches.  
The layers can be selected using checkboxes in the file tree and then created according to the details in the xml file.
The xml file has an additional entry not used by the TMR customisation which is the name of the linetype file used in the select layer.  The application will ensure this linetype is loaded into the drawing before the layer is created.
There is also a Customisation form which can be used to edit the XML file and also import layer configurations form an AutoCAD Layer State (.las) file.

\subsection intro_sec_4 Standards
The standards section is a file tree constructed from the contents of a folder on the Company file system.  
The application searched recursivly from the root folder for all PDF files and lists them in a file tree.

\subsection intro_sec_5 About
The about  section contains information about the application including links to licences and the software source code.

*/

