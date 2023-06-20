Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.IO

Namespace OWASP.WebGoat.NET
    Public Class IOHelper
        Public Shared Function ReadAllFromFile(ByVal path As String) As String
            Dim fs As New FileStream(path, FileMode.OpenOrCreate, FileAccess.Read)
            Dim sr As New StreamReader(fs)
            Dim data As String = sr.ReadToEnd()
            sr.Close()
            Return data
        End Function
    End Class
End Namespace