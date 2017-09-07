Imports System.Security.Claims
Imports System.Threading.Tasks
Imports Microsoft.AspNet.Identity
Imports Microsoft.AspNet.Identity.Owin
Imports System.Data
Imports System.Data.SqlClient


Public Class DscUser
    Implements IUser, IUser(Of String)

    Public ReadOnly Property Id As String Implements IUser(Of String).Id
        Get
            Return UserID.ToString
        End Get
    End Property

    Private _userid As String = ""
    Public Property UserID() As String
        Get
            Return _userid
        End Get
        Set(value As String)
            _userid = value
        End Set
    End Property


    Private _username As String = ""
    Public Property UserName As String Implements IUser(Of String).UserName
        Get
            Return _username
        End Get
        Set(value As String)
            _username = value
        End Set
    End Property

    Private _email As String = ""
    Public Property Email() As String
        Get
            Return _email
        End Get
        Set(value As String)
            _email = value
        End Set
    End Property

    Private _emailconfirmed As Boolean = False
    Public Property EmailConfirmed() As Boolean
        Get
            Return _emailconfirmed
        End Get
        Set(value As Boolean)
            _emailconfirmed = value
        End Set
    End Property

    Private _passwordhash As String = ""
    Public Property PasswordHash() As String
        Get
            Return _passwordhash
        End Get
        Set(value As String)
            _passwordhash = value
        End Set
    End Property

    Private _securitystamp As String
    Public Property SecurityStamp() As String
        Get
            Return _securitystamp
        End Get
        Set(ByVal value As String)
            _securitystamp = value
        End Set
    End Property

    Private _lockoutenabled As Boolean = False
    Public Property IsLocked() As Boolean
        Get
            Return _lockoutenabled
        End Get
        Set(value As Boolean)
            _lockoutenabled = value
        End Set
    End Property

    Private _lockoutenddateutc As Nullable(Of DateTimeOffset) = Nothing
    Public Property LockoutEndDateUTC() As Nullable(Of DateTimeOffset)
        Get
            Return _lockoutenddateutc
        End Get
        Set(value As Nullable(Of DateTimeOffset))
            _lockoutenddateutc = value
        End Set
    End Property

    Public Async Function GenerateUserIdentityAsync(manager As UserManager(Of DscUser)) As Task(Of ClaimsIdentity)
        ' Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
        Dim userIdentity = Await manager.CreateIdentityAsync(Me, DefaultAuthenticationTypes.ApplicationCookie)
        ' Add custom user claims here
        Return userIdentity
    End Function


End Class