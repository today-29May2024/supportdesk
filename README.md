# Support Desk (a little ticketing program, part of assessment)

**Note:** Aside from being compressed and sent to the hiring party, this source code also hosting in GitHub for further reference and continuously improve and add more features and may be archive in near future, and you know what? This small tool about ticketing is built with the help or guides from large language model chatbot as tool like reference or lookup on information about particular technology and the old way would be refering to blog post articles, tutorial and web search on the related issues during development process from forum or public and open material no matter what format it is. At the end of the day it was about to create things based on the need and requirements specified and at the same time puzzling around the technical problems raise during the software creation.

## A) How to run the backend and frontend locally, including database setup?

### 1. Get the source code
The backend and front-end of this small ticketing tool (not a complete application) is under one repository and because it is hosting on the GitHub, you can definitely clone it using git clone with the https url shown under local tab from the green 'Code' button dropdown or download the zip file for the source code. 

### 2. Start the backend
Open your terminal or command prompt and navigate to the **supportdesk-main** directory and type and enter the following command to start the ASP.NET Web API backend:

``` dotnet run --project backend/SupportDesk.Api/SupportDesk.Api.csproj ```

### 3. Start the front-end
Open another terminal or command prompt session and navigate to the **supportdesk-main\frontend\support-desk-ui** directory and type and enter the following command to start Angular front-end:

``` ng serve ```

### 4. Launch this small ticking tool with your browser of choice
Start the internet browser program and copy the address http://localhost:4200 in the address bar and hit enter to see the user interface showing and you can try to play around with it.

For database setup, see section **C) 1. for assumptions made**.

## B) Where the business rules placed and why?

The business rules or the program logic is sit inside the TicketServie.cs file under the Service folder of SupportDesk.Api project and reason to do that is to centralize the code in one place for easier lookup currently.

## C) Assumptions made

1. The database used is localdb from SQL Server that assume you have it installed on your operating system.

## D) Things to improve or add over time.

Add more unit test suitably and complete the requirements in the assessment and may have several branch for try out new things or feature from C# language and ASP.NET or add specific things I want to explore over time about using .NET tech stack. There wil be a listed things as table about it from time to time.

## E) Roughly duration the assignment took.
About 3 hours

This README.md file may be edited from time to time to reflect the changes on the source code or things that the code reader may need to know or want to know.
