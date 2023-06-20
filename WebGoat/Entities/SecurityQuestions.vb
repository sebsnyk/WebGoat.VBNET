Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema

Namespace OWASP.WebGoat.NET.Entities
    Public Partial Class SecurityQuestions
        <Key>
        <DatabaseGenerated(DatabaseGeneratedOption.None)>
        Public Property question_id As Short

        <Required>
        <StringLength(400)>
        Public Property question_text As String
    End Class
End Namespace