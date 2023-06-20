Imports System.Data

Namespace OWASP.WebGoat.NET.App_Code.DB
    Public Class DummyDbProvider
        Implements IDbProvider

        Public Function TestConnection() As Boolean Implements IDbProvider.TestConnection
            Return True
        End Function

        Public Property DbConfigFile As ConfigFile Implements IDbProvider.DbConfigFile

        Public Function GetCatalogData() As DataSet Implements IDbProvider.GetCatalogData
            Return Nothing
        End Function

        Public Function IsValidCustomerLogin(ByVal email As String, ByVal password As String) As Boolean Implements IDbProvider.IsValidCustomerLogin
            Return False
        End Function

        Public Function IsAdminCustomerLogin(ByVal email As String) As Boolean Implements IDbProvider.IsAdminCustomerLogin
            Return False
        End Function

        Public Function CreateCustomer(ByVal name As String, ByVal email As String, ByVal password As String, ByVal isAdmin As Boolean, ByVal question As Integer, ByVal answer As String) As Boolean Implements IDbProvider.CreateCustomer
            Return False
        End Function

        Public Function RecreateGoatDb() As Boolean Implements IDbProvider.RecreateGoatDb
            Return False
        End Function

        Public Function GetCustomerEmail(ByVal customerNumber As String) As String Implements IDbProvider.GetCustomerEmail
            Return String.Empty
        End Function

        Public Function GetCustomerDetails(ByVal customerNumber As String) As DataSet Implements IDbProvider.GetCustomerDetails
            Return Nothing
        End Function

        Public Function GetOffice(ByVal city As String) As DataSet Implements IDbProvider.GetOffice
            Return Nothing
        End Function

        Public Function GetMessages(ByVal customerLogin As String) As DataSet Implements IDbProvider.GetMessages
            Return Nothing
        End Function

        Public Function GetComments(ByVal productCode As String) As DataSet Implements IDbProvider.GetComments
            Return Nothing
        End Function

        Public Function AddComment(ByVal productCode As String, ByVal email As String, ByVal comment As String) As String Implements IDbProvider.AddComment
            Return String.Empty
        End Function

        Public Function UpdateCustomerPassword(ByVal customerNumber As Integer, ByVal password As String) As String Implements IDbProvider.UpdateCustomerPassword
            Return String.Empty
        End Function

        Public Function GetSecurityQuestionAndAnswer(ByVal email As String) As String() Implements IDbProvider.GetSecurityQuestionAndAnswer
            Return Nothing
        End Function

        Public Function GetPasswordByEmail(ByVal email As String) As String Implements IDbProvider.GetPasswordByEmail
            Return String.Empty
        End Function

        Public Function GetUsers() As DataSet Implements IDbProvider.GetUsers
            Return Nothing
        End Function

        Public Function GetOrders(ByVal customerID As Integer) As DataSet Implements IDbProvider.GetOrders
            Return Nothing
        End Function

        Public Function GetProductDetails(ByVal productCode As String) As DataSet Implements IDbProvider.GetProductDetails
            Return Nothing
        End Function

        Public Function GetOrderDetails(ByVal orderNumber As Integer) As DataSet Implements IDbProvider.GetOrderDetails
            Return Nothing
        End Function

        Public Function GetPayments(ByVal customerNumber As Integer) As DataSet Implements IDbProvider.GetPayments
            Return Nothing
        End Function

        Public Function GetProductsAndCategories() As DataSet Implements IDbProvider.GetProductsAndCategories
            Return Nothing
        End Function

        Public Function GetProductsAndCategories(ByVal catNumber As Integer) As DataSet Implements IDbProvider.GetProductsAndCategories
            Return Nothing
        End Function

        Public Function GetEmailByName(ByVal name As String) As DataSet Implements IDbProvider.GetEmailByName
            Return Nothing
        End Function

        Public Function GetEmailByCustomerNumber(ByVal num As String) As String Implements IDbProvider.GetEmailByCustomerNumber
            Return String.Empty
        End Function

        Public Function GetCustomerEmails(ByVal email As String) As DataSet Implements IDbProvider.GetCustomerEmails
            Return Nothing
        End Function

        Public ReadOnly Property Name As String Implements IDbProvider.Name
            Get
                Return "Dummy"
            End Get
        End Property
    End Class
End Namespace