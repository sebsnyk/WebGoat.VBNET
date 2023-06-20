Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema

Namespace OWASP.WebGoat.NET.Entities
    Partial Public Class Categories
        <Key>
        <DatabaseGenerated(DatabaseGeneratedOption.None)>
        Public Property catNumber As Long

        <Required>
        <StringLength(50)>
        Public Property catName As String

        <Required>
        <StringLength(2147483647)>
        Public Property catDesc As String
    End Class
End Namespace