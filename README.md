# NLog.MailKit

[![NuGet](https://img.shields.io/nuget/v/NLog.MailKit.svg)](https://www.nuget.org/packages/NLog.MailKit)
[![Build Status](https://dev.azure.com/NLogLogging/NLog/_apis/build/status/NLog.MailKit?branchName=master)](https://dev.azure.com/NLogLogging/NLog/_build/latest?definitionId=25&branchName=master)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=nlog.mailkit&metric=bugs)](https://sonarcloud.io/summary/new_code?id=nlog.mailkit)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=nlog.mailkit&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=nlog.mailkit)


Alternative Mail target for [NLog](https://github.com/nlog/nlog) using MailKit. Compatible with .NET standard 1, .NET standard 2 and .NET 4+

Including this package will replace the original mail target and has the
same options as the original mail target, see [docs of the original mailTarget](https://github.com/NLog/NLog/wiki/Mail-Target)

Notice that the original [SmtpClient](https://docs.microsoft.com/en-us/dotnet/api/system.net.mail.smtpclient) of .NET is obsolete in favor of MailKit:

![image](https://user-images.githubusercontent.com/5808377/44685633-351b0600-aa4c-11e8-9eec-48dd9fadb963.png)



Currently not implemented:

- PickupDirectory
- NTLM auth
- reading SMTP section from web.config

This library is integration tested with the [SmtpServer NuGet package](https://www.nuget.org/packages/SmtpServer/)


### How to use

1) Install the package: 

    `Install-Package NLog.MailKit` or in your csproj:

    ```xml
    <PackageReference Include="NLog.MailKit" Version="4.*" />
    ```

2) Add to your nlog.config:

    ```xml
    <extensions>
        <add assembly="NLog.MailKit"/>
    </extensions>
    ```

Use the target "mail"
and config options can be found here: https://github.com/NLog/NLog/wiki/Mail-Target

Use `skipCertificateValidation="true"` for prevent `AuthenticationException` if your remote certificate for smtpServer is invalid - not recommend! 





### License
BSD. License of MailKit is MIT

