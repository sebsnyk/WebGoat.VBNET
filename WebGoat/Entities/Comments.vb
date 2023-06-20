Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema

Namespace OWASP.WebGoat.NET.Entities
    Public Partial Class Comments
        <Key>
        <DatabaseGenerated(DatabaseGeneratedOption.None)>
        Public Property commentNumber As Long

        <Required>
        <StringLength(15)>
        Public Property productCode As String

        <Required>
        <StringLength(100)>
        Public Property email As String

        <Required>
        <StringLength(2147483647)>
        Public Property comment As String
    End Class
End Namespace