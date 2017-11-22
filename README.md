# NLog.MailKit

[![Build status](https://ci.appveyor.com/api/projects/status/nuh3pkael8ltd4bq/branch/master?svg=true)](https://ci.appveyor.com/project/nlog/nlog-mailkit/branch/master)
[![NuGet](https://img.shields.io/nuget/v/NLog.MailKit.svg)](https://www.nuget.org/packages/NLog.MailKit)

Mail target for [NLog](https://github.com/nlog/nlog) on .NET Core. 

Same name and options as the original mail target, see [docs of the original mailTarget](https://github.com/NLog/NLog/wiki/Mail-Target)

System.Net.Mail was not ported to .NET Core, so this libary is using the [MailKit library](https://github.com/jstedfast/MailKit)

Currently not implemented:

- PickupDirectory
- NTLM auth

SMTP section from web.config is also not available

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

Currently not implemented:

- PickupDirectory
- NTLM auth




### License
BSD. License of MailKit is MIT

