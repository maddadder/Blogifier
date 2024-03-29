<br>
<h3 align="center">Blogifier</h3>
<p align="center">
    Blogifier is a self-hosted open source publishing platform written in ASP.NET and Blazor WebAssembly. It can be used to quickly and easily set up a lightweight, but fully functional personal or group blog.
</p>

<br><br>
## Installation

Steps to install compiled application on the server for a self-hosting:

1. .NET Core Runtime (currently 6.0) must be installed on your host server.
2. [Download](https://github.com/maddadder/Blogifier/releases) the latest release.
3. Unzip and copy to your host server.<br>
4. Restart your website.
5. Open your website and only the first time you'll be redirected to the register page.<br> `example.com/admin/register/`
6. Register, and then log in.<br> `example.com/admin/login/`
7. Done, enjoy.

<br><br>
## Development
If you want to customize the Blogifier, or contribute:

1. [Download](https://dotnet.microsoft.com/download/dotnet) and Install .NET SDK.
2. Download, fork, or clone the repository.
3. Open the project with your favorite IDE (VS Code, Visual Studio, Atom, etc).
4. Rename appsettings.sqlite.json to appsettings.Development.json
5. Run the app with your IDE or these commands:

```
$ cd /your-local-path/Blogifier/src/Blogifier/
$ dotnet run
```
Then you can open `localhost:5000` with your browser, Also login to the admin panel `localhost:5000/admin/`.
```
username: admin@example.com
password: admin
```
<br><br>
## Db Migrations
```
cd src
dotnet-ef migrations add Init --verbose --project Blogifier.Core --startup-project Blogifier

To undo this action, use 'ef migrations remove --verbose --project Blogifier.Core --startup-project Blogifier'
```

<br><br>
## Debugging Notes
```
opening this project using vscode use:
export CLR_OPENSSL_VERSION_OVERRIDE=1.1
code
--------------------------------------------
while true; do kubectl port-forward --namespace neon-system pod/neon-system-db-0 5432:5432; done
OR on T30:
while true; do kubectl port-forward --namespace default pod/acid-minimal-cluster-0 5432:5432; done
#to backup restore db use pg-admin4. To restore make sure to select the role name
```

<br><br>
## Deploy to kubernetes

1. I had to go to istio-ingressgateway demonset and double the memory limit twice to prevent OOM errors (To 480Mi)

```
./deploy-t30.sh
OR
./deploy.sh
```

<br><br>
## Configure Continious db backups
See the postgres Readme.md, or readme-t30.md
