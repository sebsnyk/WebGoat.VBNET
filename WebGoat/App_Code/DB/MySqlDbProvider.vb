Imports System
Imports System.Data
Imports MySql.Data.MySqlClient
Imports log4net
Imports System.Reflection
Imports System.Diagnostics
Imports System.IO
Imports System.Threading

Namespace OWASP.WebGoat.NET.App_Code.DB
    Public Class MySqlDbProvider
        Inherits IDbProvider

        Private ReadOnly _connectionString As String
        Private ReadOnly _host As String
        Private ReadOnly _port As String
        Private ReadOnly _pwd As String
        Private ReadOnly _uid As String
        Private ReadOnly _database As String
        Private ReadOnly _clientExec As String
        Private ReadOnly log As ILog = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType)

        Public Sub New(ByVal configFile As ConfigFile)
            If configFile Is Nothing Then
                _connectionString = String.Empty
                Return
            End If

            If Not String.IsNullOrEmpty(configFile.[Get](DbConstants.KEY_PWD)) Then
                _connectionString = String.Format("SERVER={0};PORT={1};DATABASE={2};UID={3};PWD={4}", configFile.[Get](DbConstants.KEY_HOST), configFile.[Get](DbConstants.KEY_PORT), configFile.[Get](DbConstants.KEY_DATABASE), configFile.[Get](DbConstants.KEY_UID), configFile.[Get](DbConstants.KEY_PWD))
            Else
                _connectionString = String.Format("SERVER={0};PORT={1};DATABASE={2};UID={3}", configFile.[Get](DbConstants.KEY_HOST), configFile.[Get](DbConstants.KEY_PORT), configFile.[Get](DbConstants.KEY_DATABASE), configFile.[Get](DbConstants.KEY_UID))
            End If

            _uid = configFile.[Get](DbConstants.KEY_UID)
            _pwd = configFile.[Get](DbConstants.KEY_PWD)
            _database = configFile.[Get](DbConstants.KEY_DATABASE)
            _host = configFile.[Get](DbConstants.KEY_HOST)
            _clientExec = configFile.[Get](DbConstants.KEY_CLIENT_EXEC)
            _port = configFile.[Get](DbConstants.KEY_PORT)
        End Sub

        Public ReadOnly Property Name As String
            Get
                Return DbConstants.DB_TYPE_MYSQL
            End Get
        End Property

        Public Function TestConnection() As Boolean
            Try
                MySqlHelper.ExecuteNonQuery(_connectionString, "select * from information_schema.TABLES")
                Return True
            Catch ex As Exception
                log.[Error]("Error testing DB", ex)
                Return False
            End Try
        End Function

        Public Function GetCatalogData() As DataSet
            Using connection As MySqlConnection = New MySqlConnection(_connectionString)
                Dim da As MySqlDataAdapter = New MySqlDataAdapter("select * from Products", connection)
                Dim ds As DataSet = New DataSet()
                da.Fill(ds)
                Return ds
            End Using
        End Function

        Public Function IsAdminCustomerLogin(ByVal email As String) As Boolean
            Throw New NotImplementedException()
        End Function

        Public Function CreateCustomer(ByVal name As String, ByVal email As String, ByVal password As String, ByVal isAdmin As Boolean, ByVal question As Integer, ByVal answer As String) As Boolean
            Throw New NotImplementedException()
        End Function

        Public Function RecreateGoatDb() As Boolean
            Dim args As String

            If String.IsNullOrEmpty(_pwd) Then
                args = String.Format("--user={0} --database={1} --host={2} --port={3} -f", _uid, _database, _host, _port)
            Else
                args = String.Format("--user={0} --password={1} --database={2} --host={3} --port={4} -f", _uid, _pwd, _database, _host, _port)
            End If

            log.Info("Running recreate")
            Dim retVal1 As Integer = Math.Abs(Util.RunProcessWithInput(_clientExec, args, DbConstants.DB_CREATE_MYSQL_SCRIPT))
            Dim retVal2 As Integer = Math.Abs(Util.RunProcessWithInput(_clientExec, args, DbConstants.DB_LOAD_MYSQL_SCRIPT))
            Return Math.Abs(retVal1) + Math.Abs(retVal2) = 0
        End Function

        Public Function IsValidCustomerLogin(ByVal email As String, ByVal password As String) As Boolean
            Dim encoded_password As String = Encoder.Encode(password)
            Dim sql As String = "select * from CustomerLogin where email = '" & email & "' and password = '" & encoded_password & "';"

            Using connection As MySqlConnection = New MySqlConnection(_connectionString)
                Dim da As MySqlDataAdapter = New MySqlDataAdapter(sql, connection)
                Dim ds As DataSet = New DataSet()
                da.Fill(ds)

                Try
                    Return ds.Tables(0).Rows.Count <> 0
                Catch ex As Exception
                    log.[Error]("Error checking login", ex)
                    Throw New Exception("Error checking login", ex)
                End Try
            End Using
        End Function

        Public Function CustomCustomerLogin(ByVal email As String, ByVal password As String) As String
            Dim error_message As String = Nothing

            Try
                Dim sql As String = "select * from CustomerLogin where email = '" & email & "';"

                Using connection As MySqlConnection = New MySqlConnection(_connectionString)
                    Dim da As MySqlDataAdapter = New MySqlDataAdapter(sql, connection)
                    Dim ds As DataSet = New DataSet()
                    da.Fill(ds)

                    If ds.Tables(0).Rows.Count = 0 Then
                        error_message = "Email Address Not Found!"
                        Return error_message
                    End If

                    Dim encoded_password As String = ds.Tables(0).Rows(0)("Password").ToString()
                    Dim decoded_password As String = Encoder.Decode(encoded_password)

                    If password.Trim().ToLower() <> decoded_password.Trim().ToLower() Then
                        error_message = "Password Not Valid For This Email Address!"
                    Else
                        error_message = Nothing
                    End If
                End Using

            Catch ex As MySqlException
                log.[Error]("Error with custom customer login", ex)
                error_message = ex.Message
            Catch ex As Exception
                log.[Error]("Error with custom customer login", ex)
            End Try

            Return error_message
        End Function

        Public Function GetCustomerEmail(ByVal customerNumber As String) As String
            Dim output As String = Nothing

            Try

                Using connection As MySqlConnection = New MySqlConnection(_connectionString)
                    Dim sql As String = "select email from CustomerLogin where customerNumber = " & customerNumber
                    Dim command As MySqlCommand = New MySqlCommand(sql, connection)
                    output = command.ExecuteScalar().ToString()
                End Using

            Catch ex As Exception
                output = ex.Message
            End Try

            Return output
        End Function

        Public Function GetCustomerDetails(ByVal customerNumber As String) As DataSet
            Dim sql As String = "select Customers.customerNumber, Customers.customerName, Customers.logoFileName, Customers.contactLastName, Customers.contactFirstName, " & "Customers.phone, Customers.addressLine1, Customers.addressLine2, Customers.city, Customers.state, Customers.postalCode, Customers.country, " & "Customers.salesRepEmployeeNumber, Customers.creditLimit, CustomerLogin.email, CustomerLogin.password, CustomerLogin.question_id, CustomerLogin.answer " & "From Customers, CustomerLogin where Customers.customerNumber = CustomerLogin.customerNumber and Customers.customerNumber = " & customerNumber
            Dim ds As DataSet = New DataSet()

            Try

                Using connection As MySqlConnection = New MySqlConnection(_connectionString)
                    Dim da As MySqlDataAdapter = New MySqlDataAdapter(sql, connection)
                    da.Fill(ds)
                End Using

            Catch ex As Exception
                log.[Error]("Error getting customer details", ex)
                Throw New ApplicationException("Error getting customer details", ex)
            End Try

            Return ds
        End Function

        Public Function GetOffice(ByVal city As String) As DataSet
            Using connection As MySqlConnection = New MySqlConnection(_connectionString)
                Dim sql As String = "select * from Offices where city = @city"
                Dim da As MySqlDataAdapter = New MySqlDataAdapter(sql, connection)
                da.SelectCommand.Parameters.AddWithValue("@city", city)
                Dim ds As DataSet = New DataSet()
                da.Fill(ds)
                Return ds
            End Using
        End Function

        Public Function GetMessages(ByVal customerLogin As String) As DataSet
            Throw New NotImplementedException()
        End Function

        Public Function GetComments(ByVal productCode As String) As DataSet
            Using connection As MySqlConnection = New MySqlConnection(_connectionString)
                Dim sql As String = "select * from Comments where productCode = @productCode"
                Dim da As MySqlDataAdapter = New MySqlDataAdapter(sql, connection)
                da.SelectCommand.Parameters.AddWithValue("@productCode", productCode)
                Dim ds As DataSet = New DataSet()
                da.Fill(ds)
                Return ds
            End Using
        End Function

        Public Function AddComment(ByVal productCode As String, ByVal email As String, ByVal comment As String) As String
            Dim sql As String = "insert into Comments(productCode, email, comment) values ('" & productCode & "','" & email & "','" & comment & "');"
            Dim output As String = Nothing

            Try

                Using connection As MySqlConnection = New MySqlConnection(_connectionString)
                    connection.Open()
                    Dim command As MySqlCommand = New MySqlCommand(sql, connection)
                    command.ExecuteNonQuery()
                End Using

            Catch ex As Exception
                log.[Error]("Error adding comment", ex)
                output = ex.Message
            End Try

            Return output
        End Function

        Public Function UpdateCustomerPassword(ByVal customerNumber As Integer, ByVal password As String) As String
            Dim sql As String = "update CustomerLogin set password = '" & Encoder.Encode(password) & "' where customerNumber = " + customerNumber
            Dim output As String = Nothing

            Try

                Using connection As MySqlConnection = New MySqlConnection(_connectionString)
                    Dim command As MySqlCommand = New MySqlCommand(sql, connection)
                    Dim rows_added As Integer = command.ExecuteNonQuery()
                    log.Info("Rows Added: " & rows_added & " to comment table")
                End Using

            Catch ex As Exception
                log.[Error]("Error updating customer password", ex)
                output = ex.Message
            End Try

            Return output
        End Function

        Public Function GetSecurityQuestionAndAnswer(ByVal email As String) As String()
            Dim sql As String = "select SecurityQuestions.question_text, CustomerLogin.answer from CustomerLogin, " & "SecurityQuestions where CustomerLogin.email = '" & email & "' and CustomerLogin.question_id = " & "SecurityQuestions.question_id;"
            Dim qAndA As String() = New String(1) {}

            Using connection As MySqlConnection = New MySqlConnection(_connectionString)
                Dim da As MySqlDataAdapter = New MySqlDataAdapter(sql, connection)
                Dim ds As DataSet = New DataSet()
                da.Fill(ds)

                If ds.Tables(0).Rows.Count > 0 Then
                    Dim row As DataRow = ds.Tables(0).Rows(0)
                    qAndA(0) = row(0).ToString()
                    qAndA(1) = row(1).ToString()
                End If
            End Using

            Return qAndA
        End Function

        Public Function GetPasswordByEmail(ByVal email As String) As String
            Dim result As String = String.Empty

            Try

                Using connection As MySqlConnection = New MySqlConnection(_connectionString)
                    Dim sql As String = "select * from CustomerLogin where email = '" & email & "';"
                    Dim da As MySqlDataAdapter = New MySqlDataAdapter(sql, connection)
                    Dim ds As DataSet = New DataSet()
                    da.Fill(ds)

                    If ds.Tables(0).Rows.Count = 0 Then
                        result = "Email Address Not Found!"
                    End If

                    Dim encoded_password As String = ds.Tables(0).Rows(0)("Password").ToString()
                    Dim decoded_password As String = Encoder.Decode(encoded_password)
                    result = decoded_password
                End Using

            Catch ex As Exception
                result = ex.Message
            End Try

            Return result
        End Function

        Public Function GetUsers() As DataSet
            Using connection As MySqlConnection = New MySqlConnection(_connectionString)
                Dim sql As String = "select * from CustomerLogin;"
                Dim da As MySqlDataAdapter = New MySqlDataAdapter(sql, connection)
                Dim ds As DataSet = New DataSet()
                da.Fill(ds)
                Return ds
            End Using
        End Function

        Public Function GetOrders(ByVal customerID As Integer) As DataSet
            Using connection As MySqlConnection = New MySqlConnection(_connectionString)
                Dim sql As String = "select * from Orders where customerNumber = " & customerID
                Dim da As MySqlDataAdapter = New MySqlDataAdapter(sql, connection)
                Dim ds As DataSet = New DataSet()
                da.Fill(ds)

                If ds.Tables(0).Rows.Count = 0 Then
                    Return Nothing
                Else
                    Return ds
                End If
            End Using
        End Function

        Public Function GetProductDetails(ByVal productCode As String) As DataSet
            Dim sql As String = String.Empty
            Dim da As MySqlDataAdapter
            Dim ds As DataSet = New DataSet()

            Using connection As MySqlConnection = New MySqlConnection(_connectionString)
                sql = "select * from Products where productCode = '" & productCode & "'"
                da = New MySqlDataAdapter(sql, connection)
                da.Fill(ds, "products")
                sql = "select * from Comments where productCode = '" & productCode & "'"
                da = New MySqlDataAdapter(sql, connection)
                da.Fill(ds, "comments")
                Dim dr As DataRelation = New DataRelation("prod_comments", ds.Tables("products").Columns("productCode"), ds.Tables("comments").Columns("productCode"), False)
                ds.Relations.Add(dr)
                Return ds
            End Using
        End Function

        Public Function GetOrderDetails(ByVal orderNumber As Integer) As DataSet
            Dim sql As String = "select Customers.customerName, Orders.customerNumber, Orders.orderNumber, Products.productName, " & "OrderDetails.quantityOrdered, OrderDetails.priceEach, Products.productImage " & "from OrderDetails, Products, Orders, Customers where " & "Customers.customerNumber = Orders.customerNumber " & "and OrderDetails.productCode = Products.productCode " & "and Orders.orderNumber = OrderDetails.orderNumber " & "and OrderDetails.orderNumber = " & orderNumber

            Using connection As MySqlConnection = New MySqlConnection(_connectionString)
                Dim da As MySqlDataAdapter = New MySqlDataAdapter(sql, connection)
                Dim ds As DataSet = New DataSet()
                da.Fill(ds)

                If ds.Tables(0).Rows.Count = 0 Then
                    Return Nothing
                Else
                    Return ds
                End If
            End Using
        End Function

        Public Function GetPayments(ByVal customerNumber As Integer) As DataSet
            Using connection As MySqlConnection = New MySqlConnection(_connectionString)
                Dim sql As String = "select * from Payments where customerNumber = " & customerNumber
                Dim da As MySqlDataAdapter = New MySqlDataAdapter(sql, connection)
                Dim ds As DataSet = New DataSet()
                da.Fill(ds)

                If ds.Tables(0).Rows.Count = 0 Then
                    Return Nothing
                Else
                    Return ds
                End If
            End Using
        End Function

        Public Function GetProductsAndCategories() As DataSet
            Return GetProductsAndCategories(0)
        End Function

        Public Function GetProductsAndCategories(ByVal catNumber As Integer) As DataSet
            Dim sql As String = String.Empty
            Dim da As MySqlDataAdapter
            Dim ds As DataSet = New DataSet()
            Dim catClause As String = String.Empty
            If catNumber >= 1 Then catClause += " where catNumber = " & catNumber

            Using connection As MySqlConnection = New MySqlConnection(_connectionString)
                sql = "select * from Categories" & catClause
                da = New MySqlDataAdapter(sql, connection)
                da.Fill(ds, "categories")
                sql = "select * from Products" & catClause
                da = New MySqlDataAdapter(sql, connection)
                da.Fill(ds, "products")
                Dim dr As DataRelation = New DataRelation("cat_prods", ds.Tables("categories").Columns("catNumber"), ds.Tables("products").Columns("catNumber"), False)
                ds.Relations.Add(dr)
                Return ds
            End Using
        End Function

        Public Function GetEmailByName(ByVal name As String) As DataSet
            Dim sql As String = "select firstName, lastName, email from Employees where firstName like '" & name & "%' or lastName like '" & name & "%'"

            Using connection As MySqlConnection = New MySqlConnection(_connectionString)
                Dim da As MySqlDataAdapter = New MySqlDataAdapter(sql, connection)
                Dim ds As DataSet = New DataSet()
                da.Fill(ds)

                If ds.Tables(0).Rows.Count = 0 Then
                    Return Nothing
                Else
                    Return ds
                End If
            End Using
        End Function

        Public Function GetEmailByCustomerNumber(ByVal num As String) As String
            Dim output As String = ""

            Try
                output = CStr(MySqlHelper.ExecuteScalar(_connectionString, "select email from CustomerLogin where customerNumber = " & num))
            Catch ex As Exception
                log.[Error]("Error getting email by customer number", ex)
                output = ex.Message
            End Try

            Return output
        End Function

        Public Function GetCustomerEmails(ByVal email As String) As DataSet
            Dim sql As String = "select email from CustomerLogin where email like '" & email & "%'"

            Using connection As MySqlConnection = New MySqlConnection(_connectionString)
                Dim da As MySqlDataAdapter = New MySqlDataAdapter(sql, connection)
                Dim ds As DataSet = New DataSet()
                da.Fill(ds)

                If ds.Tables(0).Rows.Count = 0 Then
                    Return Nothing
                Else
                    Return ds
                End If
            End Using
        End Function
    End Class
End Namespace
