# Flight Aggregator API

## Task Overview

The Flight Aggregator API is a RESTful web service that provides functionality for searching and booking flights from various data sources. The goal of the API is to simplify the process of finding and booking airline tickets by providing a single point of access to flight data, as well as the ability to integrate with external systems.

Key features include:
- Retrieving information about available flights.
- Booking airline tickets.
- Merging data from multiple external APIs.

## Architecture

The API is built on the following principles and technologies:

- **ASP.NET Core**: Used for building the web API, ensuring high performance and scalability.
- **MediatR**: Applied for implementing the CQRS (Command Query Responsibility Segregation) pattern, which simplifies the management of requests and commands.
- **Dependency Injection**: Utilized for managing dependencies and simplifying testing.
- **Logging**: Implemented using the Serilog library for simple and structured logging.
- **Middleware**: Used to handle requests and responses, including logging, error handling, and validation of incoming data.
- **Swagger**: Employed for automatically generating API documentation and testing endpoints.
- **Redis**: Used for caching flight data to improve performance and reduce load on external APIs.

### Local Redis Requirement

**Please note**: The application requires a local Redis instance for caching purposes. You can set it up either using Docker or install it natively on your machine.

#### Option 1: Set Up Redis using Docker
To run Redis in a Docker container, you can execute the following command:

``bash
docker run -d -p 6379:6379 --name redis -e REDIS_PASSWORD=your_password redis
Make sure to replace your_password with a secure password if you want to set one. If you don't need authentication, you can just run:

docker run -d -p 6379:6379 --name redis redis
#### Option 2: Install Redis Natively
If you prefer to install Redis natively, you can find installation instructions on the Redis website.

Project Structure
Copy
- **/Controllers**: Contains the API controllers.
- **/BLL**: Business logic and services, including implementation of commands and queries using MediatR.
- **/Middlewares**: Custom middleware, for example, for logging.
- **/Models**: Data models and DTOs (Data Transfer Objects).
- **/Configurations**: Application configurations and settings, like for data sources.

## Installation and Setup

1. Clone the repository:
   ```bash
   git clone https://github.com/your-username/FlightAggregatorApi.git
Navigate to the project directory:

2. Navigate to the project directory:
cd FlightAggregatorApi
Install the required dependencies:

3. Install the required dependencies:
dotnet restore
Run the application:

4. Run the application:
dotnet run

5. Open Swagger UI to view available endpoints and test the API.

Usage
Provide examples of requests and responses for the API to help users understand how to use your API. For example:


POST /api/aggregator/book-flight
Content-Type: application/json

{
    "FlightId": 1,
    "Source" = "FlightSource1", 
    "Seats" = 1 
}

{
    "Success": true,
    "Message": "Booking confirmed for flight 1."
}

### Running the Client Application
To run the client application that interacts with the Flight Aggregator API, ensure the following:

1. Flight Aggregator API is running: Make sure you followed the installation and setup steps above to have the API up and running.

2. FlightSources Solution: 

* If you have a separate solution (FlightSources.sln) that emulates the data sources for airlines, ensure it is also running.
* This is crucial because the API will depend on these sources to gather flight data.
3. Configuring Connection Keys:

* Before running the client application, check the appsettings.json file in your project directories (both for the API and the client) for any necessary connection strings or API keys that may need to be configured.

Once the API and all data sources are up and running, you can start the client application to interact with the Flight Aggregator API.

## Vue.js Client Application
Overview
The frontend of the Flight Aggregator application is built using Vue.js, a progressive JavaScript framework for building user interfaces. This client application will communicate with the Flight Aggregator API to provide a user-friendly interface for searching and booking flights.

### Installation and Setup

1. Install Node.js dependencies:
Ensure you have Node.js installed on your machine. Then run:

* npm install

2. Configure API Endpoint:
Update the src/config.js (or similar configuration file) with the API endpoint where your Flight Aggregator API is running (for example, http://localhost:5000).

3. Run the Vue.js application:

npm run serve

### Key Additions:
- A section explaining the presence of `FlightSources.sln` and its importance for running examples of external data sources.
- A note on the connection keys that are stored in `appsettings.json`.

Feel free to adjust any section according to your needs! If there’s anything more you’d like to add or modify, just let me know!