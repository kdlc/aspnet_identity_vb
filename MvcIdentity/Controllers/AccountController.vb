Imports System.Web.Mvc
Imports System.Globalization
Imports System.Security.Claims
Imports System.Threading.Tasks
Imports Microsoft.AspNet.Identity
Imports Microsoft.AspNet.Identity.Owin
Imports Microsoft.Owin.Security
Imports Owin

<Authorize>
Public Class AccountController
    Inherits Controller
    Private _signInManager As DscSignInManager
        Private _userManager As DscUserManager

        Public Sub New()
        End Sub

        Public Sub New(appUserMan As DscUserManager, signInMan As DscSignInManager)
            UserManager = appUserMan
            SignInManager = signInMan
        End Sub

        Public Property SignInManager() As DscSignInManager
            Get
                Return If(_signInManager, HttpContext.GetOwinContext().[Get](Of DscSignInManager)())
            End Get
            Private Set
                _signInManager = Value
            End Set
        End Property

        Public Property UserManager() As DscUserManager
            Get
                Return If(_userManager, HttpContext.GetOwinContext().GetUserManager(Of DscUserManager)())
            End Get
            Private Set
                _userManager = Value
            End Set
        End Property

    '
    ' GET: /Account/Register
    <AllowAnonymous>
    Public Function Register() As ActionResult
        Return View()
    End Function

    '
    ' POST: /Account/Register
    <HttpPost>
    <AllowAnonymous>
    <ValidateAntiForgeryToken>
    Public Async Function Register(model As RegisterViewModel) As Task(Of ActionResult)
        If ModelState.IsValid Then
            Dim user = New DscUser() With {
                .UserName = model.Email,
                .Email = model.Email
            }

            'Dim result = Await Me.UserManager.CreateAsync(user, model.Password)
            Dim result = Await UserManager.CreateAsync(user, model.Password)



            If result.Succeeded Then
                Await SignInManager.SignInAsync(user, isPersistent:=False, rememberBrowser:=False)

                ' For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                ' Send an email with this link
                ' Dim code = Await UserManager.GenerateEmailConfirmationTokenAsync(user.Id)
                ' Dim callbackUrl = Url.Action("ConfirmEmail", "Account", New With { .userId = user.Id, .code = code }, protocol := Request.Url.Scheme)
                ' Await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=""" & callbackUrl & """>here</a>")

                Return RedirectToAction("Index", "Home")
            End If
            AddErrors(result)
        End If

        ' If we got this far, something failed, redisplay form
        Return View(model)
    End Function


    '
    ' GET: /Account/Login
    <AllowAnonymous>
        Public Function Login(returnUrl As String) As ActionResult
            ViewData!ReturnUrl = returnUrl
            Return View()
        End Function

        '
        ' POST: /Account/Login
        <HttpPost>
        <AllowAnonymous>
        <ValidateAntiForgeryToken>
        Public Async Function Login(model As LoginViewModel, returnUrl As String) As Task(Of ActionResult)
            If Not ModelState.IsValid Then
                Return View(model)
            End If

            ' This doesn't count login failures towards account lockout
            ' To enable password failures to trigger account lockout, change to shouldLockout := True
            Dim result = Await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout:=False)
            Select Case result
                Case SignInStatus.Success
                    Return RedirectToLocal(returnUrl)
                Case SignInStatus.LockedOut
                    Return View("Lockout")
                Case SignInStatus.RequiresVerification
                    Return RedirectToAction("SendCode", New With {
                        .ReturnUrl = returnUrl,
                        .RememberMe = model.RememberMe
                    })
                Case Else
                    ModelState.AddModelError("", "Invalid login attempt.")
                    Return View(model)
            End Select
        End Function

#Region "Helpers"
    ' Used for XSRF protection when adding external logins
    Private Const XsrfKey As String = "XsrfId"

    Private ReadOnly Property AuthenticationManager() As IAuthenticationManager
        Get
            Return HttpContext.GetOwinContext().Authentication
        End Get
    End Property

    Private Sub AddErrors(result As IdentityResult)
        For Each [error] In result.Errors
            ModelState.AddModelError("", [error])
        Next
    End Sub

    Private Function RedirectToLocal(returnUrl As String) As ActionResult
        If Url.IsLocalUrl(returnUrl) Then
            Return Redirect(returnUrl)
        End If
        Return RedirectToAction("Index", "Home")
    End Function

    Friend Class ChallengeResult
        Inherits HttpUnauthorizedResult
        Public Sub New(provider As String, redirectUri As String)
            Me.New(provider, redirectUri, Nothing)
        End Sub

        Public Sub New(provider As String, redirect As String, user As String)
            LoginProvider = provider
            RedirectUri = redirect
            UserId = user
        End Sub

        Public Property LoginProvider As String
        Public Property RedirectUri As String
        Public Property UserId As String

        Public Overrides Sub ExecuteResult(context As ControllerContext)
            Dim properties = New AuthenticationProperties() With {
                .RedirectUri = RedirectUri
            }
            If UserId IsNot Nothing Then
                properties.Dictionary(XsrfKey) = UserId
            End If
            context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider)
        End Sub
    End Class
#End Region


End Class