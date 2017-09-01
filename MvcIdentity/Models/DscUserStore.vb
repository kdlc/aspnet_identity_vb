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
    Implements IUserStore(Of DscUser), IUserLoginStore(Of DscUser), IUserClaimStore(Of DscUser), IUserPasswordStore(Of DscUser), IUserSecurityStampStore(Of DscUser), IUserEmailStore(Of DscUser)

    Public Function CreateAsync(user As DscUser) As Task Implements IUserStore(Of DscUser, String).CreateAsync
        Dim sSQL As String = "Insert Into Users(UserID, UserName, Email, EmailConfirmed, PasswordHash, HasPassword, SecurityStamp) values(@Id, @UserName, @Email, @EmailConfirmed, @PasswordHash, @HasPassword, @SecurityStamp);"
        Return Task.FromResult(Me.mConnection.Execute(sSQL, user))
    End Function

    Public Function UpdateAsync(user As DscUser) As Task Implements IUserStore(Of DscUser, String).UpdateAsync
        Throw New NotImplementedException()
    End Function

    Public Function DeleteAsync(user As DscUser) As Task Implements IUserStore(Of DscUser, String).DeleteAsync
        Throw New NotImplementedException()
    End Function

    Public Function FindByIdAsync(userId As String) As Task(Of DscUser) Implements IUserStore(Of DscUser, String).FindByIdAsync
        Dim sSQL As String = "Select * from Users where UserID='" & userId & "';"
        Dim _user As DscUser = Me.mConnection.Query(Of DscUser)(sSQL).FirstOrDefault
        Return Task.FromResult(_user)
    End Function

    Public Function FindByNameAsync(userName As String) As Task(Of DscUser) Implements IUserStore(Of DscUser, String).FindByNameAsync
        Dim sSQL As String = "Select * from Users where username='" & userName & "';"
        Dim _user As DscUser = Me.mConnection.Query(Of DscUser)(sSQL).FirstOrDefault
        Return Task.FromResult(_user)
        'Return Nothing
    End Function

    Public Function SetPasswordHashAsync(user As DscUser, passwordHash As String) As Task Implements IUserPasswordStore(Of DscUser, String).SetPasswordHashAsync
        'Return Task.FromResult(Of String)(user.PasswordHash = passwordHash)
        user.PasswordHash = passwordHash
        Return Task.FromResult(Of String)("")
    End Function

    Public Function GetPasswordHashAsync(user As DscUser) As Task(Of String) Implements IUserPasswordStore(Of DscUser, String).GetPasswordHashAsync
        Return Task.FromResult(Of String)(user.PasswordHash)
    End Function

    Public Function SetEmailAsync(user As DscUser, email As String) As Task Implements IUserEmailStore(Of DscUser, String).SetEmailAsync
        user.Email = email
        Return Task.FromResult(Of String)(user.Email)
    End Function

    Public Function GetEmailAsync(user As DscUser) As Task(Of String) Implements IUserEmailStore(Of DscUser, String).GetEmailAsync
        Return Task.FromResult(Of String)(user.Email)
    End Function

    Public Function GetEmailConfirmedAsync(user As DscUser) As Task(Of Boolean) Implements IUserEmailStore(Of DscUser, String).GetEmailConfirmedAsync
        Return Task.FromResult(Of Boolean)(user.EmailConfirmed)
    End Function

    Public Function SetEmailConfirmedAsync(user As DscUser, confirmed As Boolean) As Task Implements IUserEmailStore(Of DscUser, String).SetEmailConfirmedAsync
        user.EmailConfirmed = confirmed
        Return Task.FromResult(Of Boolean)(user.EmailConfirmed)
    End Function

    Public Function FindByEmailAsync(email As String) As Task(Of DscUser) Implements IUserEmailStore(Of DscUser, String).FindByEmailAsync
        Dim sSQL As String = "Select * from Users where email='" & email & "';"
        Dim _user As DscUser = Me.mConnection.Query(Of DscUser)(sSQL).FirstOrDefault
        Return Task.FromResult(_user)
    End Function

    Public Function HasPasswordAsync(user As DscUser) As Task(Of Boolean) Implements IUserPasswordStore(Of DscUser, String).HasPasswordAsync
        Throw New NotImplementedException()
    End Function

    Public Function SetSecurityStampAsync(user As DscUser, stamp As String) As Task Implements IUserSecurityStampStore(Of DscUser, String).SetSecurityStampAsync
        Return Task.FromResult(Of String)(user.SecurityStamp = stamp)
    End Function

    Public Function GetSecurityStampAsync(user As DscUser) As Task(Of String) Implements IUserSecurityStampStore(Of DscUser, String).GetSecurityStampAsync
        Return Task.FromResult(Of String)(user.SecurityStamp)
    End Function

    Public Function AddLoginAsync(user As DscUser, login As UserLoginInfo) As Task Implements IUserLoginStore(Of DscUser, String).AddLoginAsync
        Throw New NotImplementedException()
    End Function

    Public Function RemoveLoginAsync(user As DscUser, login As UserLoginInfo) As Task Implements IUserLoginStore(Of DscUser, String).RemoveLoginAsync
        Throw New NotImplementedException()
    End Function

    Public Function GetLoginsAsync(user As DscUser) As Task(Of IList(Of UserLoginInfo)) Implements IUserLoginStore(Of DscUser, String).GetLoginsAsync
        Throw New NotImplementedException()
    End Function

    Public Function FindAsync(login As UserLoginInfo) As Task(Of DscUser) Implements IUserLoginStore(Of DscUser, String).FindAsync
        Throw New NotImplementedException()
    End Function

    Public Function GetClaimsAsync(user As DscUser) As Task(Of IList(Of Claim)) Implements IUserClaimStore(Of DscUser, String).GetClaimsAsync
        Throw New NotImplementedException()
    End Function

    Public Function AddClaimAsync(user As DscUser, claim As Claim) As Task Implements IUserClaimStore(Of DscUser, String).AddClaimAsync
        Throw New NotImplementedException()
    End Function

    Public Function RemoveClaimAsync(user As DscUser, claim As Claim) As Task Implements IUserClaimStore(Of DscUser, String).RemoveClaimAsync
        Throw New NotImplementedException()
    End Function

#Region "IDisposable Support"
    Private sConnectionString As String = ""
    Private mConnection As IDbConnection
    Private disposedValue As Boolean ' To detect redundant calls

    Public Sub New(ByVal connectionString As String)
        If String.IsNullOrWhiteSpace(connectionString) Then
            Throw New ArgumentException("Connection String is Empty.")
        End If

        sConnectionString = connectionString
        mConnection = New SqlConnection(sConnectionString)
        mConnection.Open()

    End Sub

    Public Sub New()
        sConnectionString = ConfigurationManager.ConnectionStrings("MvcTestConnection").ConnectionString
        mConnection = New SqlConnection(sConnectionString)
        mConnection.Open()

        If mConnection.State = ConnectionState.Open Then
            Debug.Print("Connection to Database is now Open...")
        Else
            Debug.Print("Can't connect to Database...")
        End If


    End Sub

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
                If mConnection.State = ConnectionState.Open Then
                    mConnection.Close()
                End If
                mConnection = Nothing
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