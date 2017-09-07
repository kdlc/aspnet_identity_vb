Imports System.Security.Claims
Imports System.Threading.Tasks
Imports Microsoft.AspNet.Identity
Imports Microsoft.AspNet.Identity.Owin


Imports System.Data
Imports System.Data.SqlClient
Imports Dapper
Imports System.Configuration
Imports MvcIdentity

Public Class DscUserStore
    Implements IUserStore(Of DscUser), IUserLoginStore(Of DscUser), IUserPasswordStore(Of DscUser), IUserSecurityStampStore(Of DscUser), IUserEmailStore(Of DscUser), IUserLockoutStore(Of DscUser, String), IUserTwoFactorStore(Of DscUser, String)

    Private sConnectionString As String = ""
    Private mConnection As IDbConnection
    Private disposedValue As Boolean ' To detect redundant calls

    Public Sub New()
        sConnectionString = ConfigurationManager.ConnectionStrings("MvcTestConnection").ConnectionString
    End Sub

#Region "IUserStore"

    Public Function CreateAsync(user As DscUser) As Task Implements IUserStore(Of DscUser, String).CreateAsync
        If IsNothing(user) = False Then
            user.UserID = Guid.NewGuid().ToString
            Dim sSQL As String = "Insert Into Users(UserID, UserName, PasswordHash, SecurityStamp) values(@UserID, @UserName, @PasswordHash, @SecurityStamp);"
            Using conn As New SqlConnection(sConnectionString)
                conn.Open()
                conn.Execute(sSQL, user)
            End Using
        Else
            Throw New ArgumentException("user is nothing")
        End If
        Return Task.FromResult(0)
    End Function

    Public Function UpdateAsync(user As DscUser) As Task Implements IUserStore(Of DscUser, String).UpdateAsync
        If IsNothing(user) = False Then
            Dim sSQL As String = "update users set UserName=@UserName, PasswordHash=@PasswordHash, SecurityStamp=@SecurityStamp where UserID=@UserID;"
            Using conn As New SqlConnection(sConnectionString)
                conn.Open()
                conn.Execute(sSQL, user)
            End Using
        Else
            Throw New ArgumentException("user is nothing")
        End If
        Return Task.FromResult(0)
    End Function

    Public Function DeleteAsync(user As DscUser) As Task Implements IUserStore(Of DscUser, String).DeleteAsync
        If IsNothing(user) = False Then
            Dim sSQL As String = "delete from users where UserID=@UserID;"
            Using conn As New SqlConnection(sConnectionString)
                conn.Open()
                conn.Execute(sSQL, user)
            End Using
        Else
            Throw New ArgumentException("user is nothing")
        End If
        Return Task.FromResult(0)
    End Function

    Public Function FindByIdAsync(userId As String) As Task(Of DscUser) Implements IUserStore(Of DscUser, String).FindByIdAsync

        If (String.IsNullOrWhiteSpace(userId)) Then
            Throw New ArgumentNullException("userId")
        End If

        Dim parsedGuid As New Guid
        If (Not Guid.TryParse(userId, parsedGuid)) Then
            Throw New ArgumentOutOfRangeException("userId", "invalid user id...")
        End If

        Dim task As Task =
            Task.Factory.StartNew(
            Function()
                Using conn As New SqlConnection(sConnectionString)
                    conn.Open()
                    Return conn.Query(Of DscUser)("select CONVERT(nvarchar(36), userId) as userid, UserName, PasswordHash, Convert(nvarchar(36), SecurityStamp) as SecurityStamp from users where UserId=@userId", New With {userId}).SingleOrDefault
                End Using
            End Function
        )

        Return task

    End Function

    Public Function FindByNameAsync(userName As String) As Task(Of DscUser) Implements IUserStore(Of DscUser, String).FindByNameAsync

        If String.IsNullOrWhiteSpace(userName) = True Then
            Throw New ArgumentNullException("userName")
        End If

        Dim task As Task =
            Task.Factory.StartNew(
            Function()
                Using conn As New SqlConnection(sConnectionString)
                    conn.Open()
                    Return conn.Query(Of DscUser)("select CONVERT(nvarchar(36), userId) as userid, UserName, PasswordHash, Convert(nvarchar(36), SecurityStamp) as SecurityStamp from users where lower(UserName)=lower(@userName)", New With {userName}).SingleOrDefault
                End Using
            End Function
        )

        Return task

    End Function

#End Region

#Region "IUserLoginStore"

    Public Function AddLoginAsync(user As DscUser, login As UserLoginInfo) As Task Implements IUserLoginStore(Of DscUser, String).AddLoginAsync

        Dim tsk As Task =
            Task.Factory.StartNew(
            Function()
                Dim result As Boolean = False
                Using connection As New SqlConnection(sConnectionString)
                    connection.Execute("insert into ExternalLogins(ExternalLoginId, UserId, LoginProvider, ProviderKey) values(@externalLoginId, @userId, @loginProvider, @providerKey)", New With {Key .externalLoginId = Guid.NewGuid(), .userId = user.UserID, .loginProvider = login.LoginProvider, .providerKey = login.ProviderKey})
                    result = True
                End Using
                Return result
            End Function)
        Return tsk

    End Function

    Public Function RemoveLoginAsync(user As DscUser, login As UserLoginInfo) As Task Implements IUserLoginStore(Of DscUser, String).RemoveLoginAsync

        Dim tsk As Task =
            Task.Factory.StartNew(
            Function()
                Dim result As Boolean = False
                Using connection As New SqlConnection(sConnectionString)
                    connection.Execute("delete from ExternalLogins where UserId = @userId and LoginProvider = @loginProvider and ProviderKey = @providerKey", New With {Key .userId = user.UserID, .loginProvider = login.LoginProvider, .providerKey = login.ProviderKey})
                    result = True
                End Using
                Return result
            End Function)
        Return tsk

    End Function

    Public Function GetLoginsAsync(user As DscUser) As Task(Of IList(Of UserLoginInfo)) Implements IUserLoginStore(Of DscUser, String).GetLoginsAsync
        Dim _results As List(Of UserLoginInfo)

        If IsNothing(user) = True Then
            Throw New ArgumentNullException("user")
        End If

        Dim task As Task =
            Task.Factory.StartNew(
            Function()
                Using conn As New SqlConnection(sConnectionString)
                    conn.Open()
                    Dim sSQL As String = "select LoginProvider, ProviderKey From ExternalLogins where UserId='" & user.UserID & "';"
                    _results = conn.Query(Of UserLoginInfo)(sSQL).ToList
                End Using
                Return _results
            End Function
        )

        Return task

    End Function

    Public Function FindAsync(login As UserLoginInfo) As Task(Of DscUser) Implements IUserLoginStore(Of DscUser, String).FindAsync
        If IsNothing(login) = True Then
            Throw New ArgumentNullException("login")
        End If

        Dim task As Task =
            Task.Factory.StartNew(
            Function()
                Using conn As New SqlConnection(sConnectionString)
                    conn.Open()
                    Return conn.Query(Of DscUser)("Select u.* from Users u inner join ExternalLogins l on l.userid=u.userid where l.loginprovider=@loginProvider and l.ProviderKey=@providerKey;", login).SingleOrDefault
                End Using
            End Function
        )

        Return task


    End Function

#End Region

#Region "IUserPasswordStore"

    Public Function SetPasswordHashAsync(user As DscUser, passwordHash As String) As Task Implements IUserPasswordStore(Of DscUser, String).SetPasswordHashAsync

        If IsNothing(user) Then
            Throw New ArgumentNullException("user")
        End If

        If IsNothing(passwordHash) Then
            Throw New ArgumentNullException("passwordhash")
        End If

        user.PasswordHash = passwordHash
        Return Task.FromResult(0)

    End Function

    Public Function GetPasswordHashAsync(user As DscUser) As Task(Of String) Implements IUserPasswordStore(Of DscUser, String).GetPasswordHashAsync
        If IsNothing(user) Then
            Throw New ArgumentNullException
        End If
        Return Task.FromResult(user.PasswordHash)
    End Function

    Public Function HasPasswordAsync(user As DscUser) As Task(Of Boolean) Implements IUserPasswordStore(Of DscUser, String).HasPasswordAsync
        Return Task.FromResult(Not String.IsNullOrEmpty(user.PasswordHash))
    End Function
#End Region

#Region "IUSerSecurityStampStore"

    Public Function SetSecurityStampAsync(user As DscUser, stamp As String) As Task Implements IUserSecurityStampStore(Of DscUser, String).SetSecurityStampAsync
        If IsNothing(user) = True Then
            Throw New ArgumentNullException("user")
        End If
        user.SecurityStamp = stamp
        Return Task.FromResult(0)
    End Function

    Public Function GetSecurityStampAsync(user As DscUser) As Task(Of String) Implements IUserSecurityStampStore(Of DscUser, String).GetSecurityStampAsync
        If IsNothing(user) = True Then
            Throw New ArgumentNullException("user")
        End If
        Return Task.FromResult(user.SecurityStamp)
    End Function

#End Region

#Region "IUserEmailStore"
    Public Function SetEmailAsync(user As DscUser, email As String) As Task Implements IUserEmailStore(Of DscUser, String).SetEmailAsync
        If IsNothing(user) Then
            Throw New ArgumentNullException("user")
        End If
        If String.IsNullOrEmpty(email) Then
            Throw New ArgumentNullException("email")
        End If
        Return Task.FromResult(user.Email)
    End Function

    Public Function GetEmailAsync(user As DscUser) As Task(Of String) Implements IUserEmailStore(Of DscUser, String).GetEmailAsync
        If IsNothing(user) Then
            Throw New ArgumentNullException("user")
        End If
        Return Task.FromResult(user.Email)
    End Function

    Public Function GetEmailConfirmedAsync(user As DscUser) As Task(Of Boolean) Implements IUserEmailStore(Of DscUser, String).GetEmailConfirmedAsync
        If IsNothing(user) = True Then
            Throw New ArgumentNullException("user")
        End If
        Return Task.FromResult(user.EmailConfirmed)
    End Function

    Public Function SetEmailConfirmedAsync(user As DscUser, confirmed As Boolean) As Task Implements IUserEmailStore(Of DscUser, String).SetEmailConfirmedAsync
        If IsNothing(user) Then
            Throw New ArgumentNullException("user")
        End If
        If String.IsNullOrEmpty(confirmed) Then
            Throw New ArgumentNullException("confirmed")
        End If
        Return Task.FromResult(user.EmailConfirmed)
    End Function

    Public Function FindByEmailAsync(email As String) As Task(Of DscUser) Implements IUserEmailStore(Of DscUser, String).FindByEmailAsync
        If String.IsNullOrEmpty(email) = True Then
            Throw New ArgumentNullException("email")
        End If
        Dim task As Task =
            Task.Factory.StartNew(
            Function()
                Using conn As New SqlConnection(sConnectionString)
                    conn.Open()
                    Return conn.Query(Of DscUser)("select CONVERT(nvarchar(36), userId) as userid, UserName, PasswordHash, Convert(nvarchar(36), SecurityStamp) as SecurityStamp from Users where lower(UserName)=lower(@email)", New With {email}).SingleOrDefault
                End Using
            End Function
        )
        Return task
    End Function
#End Region

#Region "IUserLockStore"

    Public Function GetLockoutEndDateAsync(user As DscUser) As Task(Of DateTimeOffset) Implements IUserLockoutStore(Of DscUser, String).GetLockoutEndDateAsync
        If IsNothing(user) Then
            Throw New ArgumentNullException("user")
        End If
        Dim lockoutdate As DateTimeOffset
        If user.LockoutEndDateUTC.HasValue Then
            lockoutdate = user.LockoutEndDateUTC.Value
        Else
            lockoutdate = New DateTimeOffset(DateTime.Now.AddMinutes(-5))
        End If
        Return Task.FromResult(lockoutdate)
    End Function

    Public Function SetLockoutEndDateAsync(user As DscUser, lockoutEnd As DateTimeOffset) As Task Implements IUserLockoutStore(Of DscUser, String).SetLockoutEndDateAsync
        user.LockoutEndDateUTC = lockoutEnd
        user.IsLocked = True
        Return Task.FromResult(0)
    End Function

    Public Function IncrementAccessFailedCountAsync(user As DscUser) As Task(Of Integer) Implements IUserLockoutStore(Of DscUser, String).IncrementAccessFailedCountAsync
        Return Task.FromResult(1)
    End Function

    Public Function ResetAccessFailedCountAsync(user As DscUser) As Task Implements IUserLockoutStore(Of DscUser, String).ResetAccessFailedCountAsync
        Return Task.FromResult(0)
    End Function

    Public Function GetAccessFailedCountAsync(user As DscUser) As Task(Of Integer) Implements IUserLockoutStore(Of DscUser, String).GetAccessFailedCountAsync
        Return Task.FromResult(0)
    End Function

    Public Function GetLockoutEnabledAsync(user As DscUser) As Task(Of Boolean) Implements IUserLockoutStore(Of DscUser, String).GetLockoutEnabledAsync
        Return Task.FromResult(False)
    End Function

    Public Function SetLockoutEnabledAsync(user As DscUser, enabled As Boolean) As Task Implements IUserLockoutStore(Of DscUser, String).SetLockoutEnabledAsync
        user.IsLocked = IIf(enabled = True, False, True)
        Return Task.FromResult(0)
    End Function

#End Region

#Region "Two Factor Authentication"

    Public Function SetTwoFactorEnabledAsync(user As DscUser, enabled As Boolean) As Task Implements IUserTwoFactorStore(Of DscUser, String).SetTwoFactorEnabledAsync
        Return Task.FromResult(0)
    End Function

    Public Function GetTwoFactorEnabledAsync(user As DscUser) As Task(Of Boolean) Implements IUserTwoFactorStore(Of DscUser, String).GetTwoFactorEnabledAsync
        Return Task.FromResult(False)
    End Function

#End Region

#Region "IDisposable Support"
    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).              
            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
            ' TODO: set large fields to null.
        End If
        disposedValue = True
    End Sub

    ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
    'Protected Overrides Sub Finalize()
    '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        Dispose(True)
        ' TODO: uncomment the following line if Finalize() is overridden above.
        ' GC.SuppressFinalize(Me)
    End Sub

#End Region

End Class