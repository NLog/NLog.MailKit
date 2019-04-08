# NLog.MailKit

[![Build status](https://ci.appveyor.com/api/projects/status/nuh3pkael8ltd4bq/branch/master?svg=true)](https://ci.appveyor.com/project/nlog/nlog-mailkit/branch/master)
[![NuGet](https://img.shields.io/nuget/v/NLog.MailKit.svg)](https://www.nuget.org/packages/NLog.MailKit)

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
    <PackageReference Include="NLog.MailKit" Version="3.0.1" />
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

