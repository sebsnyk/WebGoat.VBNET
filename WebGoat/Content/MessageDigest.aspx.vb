Imports System
Imports System.Web
Imports System.Web.UI
Imports OWASP.WebGoat.NET.App_Code
Imports log4net
Imports System.Reflection

Namespace OWASP.WebGoat.NET.Content

    Partial Public Class MessageDigest
        Inherits System.Web.UI.Page

        Private ReadOnly _log As ILog = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType)

        Private Const MSG As String = "Well done! You can now consider yourself an expert hacker! Well almost. Surely this is an easy digest to break!"

        Public Sub Page_Load(sender As Object, args As EventArgs)
            lblDigest.Text = WeakMessageDigest.GenerateWeakDigest(MSG)
        End Sub

        Public Sub btnDigest_Click(sender As Object, args As EventArgs)
            Dim result As String = WeakMessageDigest.GenerateWeakDigest(txtBoxMsg.Text)

            _log.Info(String.Format("Result for {0} is: {1}", txtBoxMsg.Text, result))
            lblResultDigest.Text = result
        End Sub
    End Class
End Namespace