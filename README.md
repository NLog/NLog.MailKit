# NLog.MailKit

[![Build status](https://ci.appveyor.com/api/projects/status/nuh3pkael8ltd4bq/branch/master?svg=true)](https://ci.appveyor.com/project/nlog/nlog-mailkit/branch/master)
[![NuGet](https://img.shields.io/nuget/v/NLog.MailKit.svg)](https://www.nuget.org/packages/NLog.MailKit)

Alternaive Mail target for [NLog](https://github.com/nlog/nlog) using MailKit. Compatible with .NET standard 1, .NET standard 2 and .NET 4+

Including this package will replace the original mail target and has the
same options as the original mail target, see [docs of the original mailTarget](https://github.com/NLog/NLog/wiki/Mail-Target)


Currently not implemented:

- PickupDirectory
- NTLM auth
- reading SMTP section from web.config

This library is unit tested with the [SmtpServer NuGet package](https://www.nuget.org/packages/SmtpServer/)


### How to use
Install the package: `Install-Package NLog.MailKit` and add to your nlog.config:

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

