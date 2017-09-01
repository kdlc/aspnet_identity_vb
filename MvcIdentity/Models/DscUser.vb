Imports System.Security.Claims
Imports System.Threading.Tasks
Imports Microsoft.AspNet.Identity
Imports Microsoft.AspNet.Identity.Owin
Imports System.Data
Imports System.Data.SqlClient


Public Class DscUser
    Implements IUser, IUser(Of String)

    Dim new_id As String = ""

    Public Sub New()

    End Sub

    'Public ReadOnly Property Id As String Implements IUser(Of String).Id
    '    Get
    '        Return new_id
    '    End Get
    'End Property
    Dim _id As String = ""
    Public Property Id() As String Implements IUser(Of String).Id
        Get
            Return _id
        End Get
        Set(value As String)
            _id = value 
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

    Private _email As String
    Public Property Email() As String
        Get
            Return _email
        End Get
        Set(ByVal value As String)
            _email = value
        End Set
    End Property

    Private _emailconfirmed As Boolean
    Public Property EmailConfirmed() As Boolean
        Get
            Return _emailconfirmed
        End Get
        Set(ByVal value As Boolean)
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

    Private _haspassword As Boolean = False
    Public Property HasPassword() As Boolean
        Get
            Return _haspassword
        End Get
        Set(value As Boolean)
            _haspassword = value
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

    Public Async Function GenerateUserIdentityAsync(manager As UserManager(Of DscUser)) As Task(Of ClaimsIdentity)
        ' Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
        Dim userIdentity = Await manager.CreateIdentityAsync(Me, DefaultAuthenticationTypes.ApplicationCookie)
        ' Add custom user claims here
        Return userIdentity
    End Function


End Class