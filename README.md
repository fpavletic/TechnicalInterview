# TechnicalInterview02

The application can be run by pressing F5 in VS, Shift+F9 in Rider, or by building it and running the WebAPI exe.

If ran using VS or Rider, you'll be redirected to a Swagger page detailing the endpoints available. If ran using the exe you can find the OpenAPI definition in the repository root directory under the name ReleaseBox.api.

In addition to the requested endpoints there's an additional one that returns all file system entries in the provided directory. It seemed neccessary for the user to navigate their filesystem.

The application creates an Sqlite database file at ./ReleaseBox.sqlite and a log file at ./ReleaseBox.log. Please make sure it is allowed to write and read from that location.
