Imports System.Threading.Tasks
Imports System.Security.Claims
Imports Microsoft.AspNet.Identity
Imports Microsoft.AspNet.Identity.Owin
Imports Microsoft.Owin
Imports Microsoft.Owin.Security

Public Class EmailService
    Implements IIdentityMessageService

    Public Function SendAsync(message As IdentityMessage) As Task Implements IIdentityMessageService.SendAsync
        ' Plug in your email service here to send an email.
        Return Task.FromResult(0)
    End Function
End Class

Public Class SmsService
    Implements IIdentityMessageService

    Public Function SendAsync(message As IdentityMessage) As Task Implements IIdentityMessageService.SendAsync
        ' Plug in your SMS service here to send a text message.
        Return Task.FromResult(0)
    End Function
End Class

' Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.
Public Class DscUserManager
    Inherits UserManager(Of DscUser)

    Public Sub New(store As IUserStore(Of DscUser))
        MyBase.New(store)
    End Sub

    Public Shared Function Create(options As IdentityFactoryOptions(Of DscUserManager), context As IOwinContext) As DscUserManager
        'Dim manager = New DscUserManager(New UserStore(Of DscUser)(context.Get(Of ApplicationDbContext)()))
        Dim manager = New DscUserManager(New DscUserStore())

        ' Configure validation logic for usernames
        manager.UserValidator = New UserValidator(Of DscUser)(manager) With {
            .AllowOnlyAlphanumericUserNames = False,
            .RequireUniqueEmail = True
        }

        ' Configure validation logic for passwords
        manager.PasswordValidator = New PasswordValidator With {
            .RequiredLength = 6,
            .RequireNonLetterOrDigit = True,
            .RequireDigit = True,
            .RequireLowercase = True,
            .RequireUppercase = True
        }

        ' Configure user lockout defaults
        manager.UserLockoutEnabledByDefault = True
        manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5)
        manager.MaxFailedAccessAttemptsBeforeLockout = 5

        ' Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
        ' You can write your own provider and plug it in here.
        manager.RegisterTwoFactorProvider("Phone Code", New PhoneNumberTokenProvider(Of DscUser) With {
                                          .MessageFormat = "Your security code is {0}"
                                      })
        manager.RegisterTwoFactorProvider("Email Code", New EmailTokenProvider(Of DscUser) With {
                                          .Subject = "Security Code",
                                          .BodyFormat = "Your security code is {0}"
                                          })
        manager.EmailService = New EmailService()
        manager.SmsService = New SmsService()
        Dim dataProtectionProvider = options.DataProtectionProvider
        If (dataProtectionProvider IsNot Nothing) Then
            manager.UserTokenProvider = New DataProtectorTokenProvider(Of DscUser)(dataProtectionProvider.Create("ASP.NET Identity"))
        End If

        Return manager
    End Function

End Class

' Configure the application sign-in manager which is used in this application.
Public Class DscSignInManager
    Inherits SignInManager(Of DscUser, String)
    Public Sub New(userManager As DscUserManager, authenticationManager As IAuthenticationManager)
        MyBase.New(userManager, authenticationManager)
    End Sub

    Public Overrides Function CreateUserIdentityAsync(user As DscUser) As Task(Of ClaimsIdentity)
        Return user.GenerateUserIdentityAsync(DirectCast(UserManager, DscUserManager))
    End Function

    Public Shared Function Create(options As IdentityFactoryOptions(Of DscSignInManager), context As IOwinContext) As DscSignInManager
        Return New DscSignInManager(context.GetUserManager(Of DscUserManager)(), context.Authentication)
    End Function
End Class
