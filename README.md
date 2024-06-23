# NLog.MailKit

[![NuGet](https://img.shields.io/nuget/v/NLog.MailKit.svg)](https://www.nuget.org/packages/NLog.MailKit)
[![Build Status](https://dev.azure.com/NLogLogging/NLog/_apis/build/status/NLog.MailKit?branchName=master)](https://dev.azure.com/NLogLogging/NLog/_build/latest?definitionId=25&branchName=master)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=nlog.mailkit&metric=bugs)](https://sonarcloud.io/summary/new_code?id=nlog.mailkit)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=nlog.mailkit&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=nlog.mailkit)
[![](https://sonarcloud.io/api/project_badges/measure?project=nlog.mailkit&metric=code_smells)](https://sonarcloud.io/project/issues?id=nlog.mailkit&resolved=false&types=CODE_SMELL) 
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=nlog.mailkit&metric=coverage)](https://sonarcloud.io/component_measures?id=nlog.mailkit&metric=coverage)

Alternative Mail target for [NLog](https://github.com/nlog/nlog) using [MailKit](https://github.com/jstedfast/MailKit). Compatible with .NET standard 2+ 

Including this package will replace the original mail target and has the
same options as the original mail target, see [docs of the original mailTarget](https://github.com/NLog/NLog/wiki/Mail-Target)

Currently not implemented:

- NTLM auth

This library is integration tested with the [SmtpServer NuGet package](https://www.nuget.org/packages/SmtpServer/)


### How to use

1) Install the package: 

    `Install-Package NLog.MailKit` or in your csproj:

    ```xml
    <PackageReference Include="NLog.MailKit" Version="5.*" />
    ```

2) Add to your nlog.config:

    ```xml
    <extensions>
        <add assembly="NLog.MailKit"/>
    </extensions>
    ```

   Alternative register from code using [fluent configuration API](https://github.com/NLog/NLog/wiki/Fluent-Configuration-API):

    ```xml
    LogManager.Setup().SetupExtensions(ext => ext.RegisterTarget<NLog.MailKit.MailTarget>());
    ```

See the [NLog Wiki](https://github.com/NLog/NLog/wiki/Mail-Target) for available options and examples.

Note that the option `skipCertificateValidation="true"` can prevent `AuthenticationException` if your remote certificate for smtpServer is invalid - not recommend!

### License
BSD. License of MailKit is MIT

