The application is divided into 6 layers:

Core - Represents the data model, but enhanced with logic for organizing the hierarchy of objects and with stronger encapsulation, so that objects are difficult or impossible to break from outside Core.

Infrastructure - Provides technical implementations for application needs (database access, file storage, messaging, etc.).
--Data - Part of Infrastructure responsible for working with the database and providing data access to Services. Also responsible for file system persistence (saving and loading files).

Services - Acts as a mediator between ViewModels and Core/Data. While Core is responsible for interactions strictly between domain objects, Services mainly manages interactions in connection with the database. It also provides DTO versions of objects for safe use in ViewModels. Full-scale actions that modify the application are executed via created commands, enabling undo/redo functionality.

ViewModels - Provide the necessary data for the UI.WPF layer, and send commands in response to user actions.

UI.WPF - The visual part of the application. Responsible only for presentation, with the minimal code required to connect ViewModels or initialize dependency injection.