Imports Owin

Public Class Startup

    Public Sub Configuration(app As IAppBuilder)
        ConfigureAuth(app)
    End Sub

End Class
