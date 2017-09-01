Imports Owin
Imports Microsoft.AspNet.Identity
Imports Microsoft.AspNet.Identity.Owin
Imports Microsoft.Owin
Imports Microsoft.Owin.Security.Cookies

Partial Public Class Startup
    Public Sub ConfigureAuth(app As IAppBuilder)

        app.CreatePerOwinContext(Of DscUserManager)(AddressOf DscUserManager.Create)
        app.CreatePerOwinContext(Of DscSignInManager)(AddressOf DscSignInManager.Create)

        app.UseCookieAuthentication(New CookieAuthenticationOptions() With {
            .AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
            .Provider = New CookieAuthenticationProvider() With {
            .OnValidateIdentity = SecurityStampValidator.OnValidateIdentity(Of DscUserManager, DscUser)(
               validateInterval:=TimeSpan.FromMinutes(30),
               regenerateIdentity:=Function(manager, user) user.generateUserIdentityAsync(manager))},
               .LoginPath = New PathString("/Account/Login")})

        app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie)


    End Sub

End Class
