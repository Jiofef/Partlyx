The application is divided into 6 layers:

Core – Represents the data model, but enhanced with logic for organizing the hierarchy of objects and with stronger encapsulation, so that objects are difficult or impossible to break from outside Core.

Data – Works with the database. Responsible for providing data from the database to Services, and for saving files to disk.

Services – Acts as a mediator between ViewModels and Core/Data. While Core is responsible for interactions strictly between domain objects, Services mainly manages interactions in connection with the database. It also provides DTO versions of objects for safe use in ViewModels. Full-scale actions that modify the application are executed via created commands, enabling undo/redo functionality.

ViewModels – Provide the necessary data for the UI.WPF layer, and send commands in response to user actions.

UI.WPF – The visual part of the application. Responsible only for presentation, with the minimal code required to connect ViewModels or initialize dependency injection.