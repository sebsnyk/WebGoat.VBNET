Imports System
Imports System.IO

Namespace OWASP.WebGoat.NET.App_Code.DB
    Public Class DbConstants
        'Keys
        Public Const KEY_DB_TYPE As String = "dbtype"
        Public Const KEY_CLIENT_EXEC As String = "client"
        Public Const KEY_HOST As String = "host"
        Public Const KEY_PORT As String = "port"
        Public Const KEY_FILE_NAME As String = "filename"
        Public Const KEY_DATABASE As String = "database"
        Public Const KEY_UID As String = "uid"
        Public Const KEY_PWD As String = "pwd"
        
        'DB Types
        Public Const DB_TYPE_MYSQL As String = "MySql"
        Public Const DB_TYPE_SQLITE As String = "Sqlite"
        Public Const CONFIG_EXT As String = "config"

        'DB Scripts
        Private Const SCRIPT_DIR As String = "DB_Scripts"
        Public Shared ReadOnly DB_CREATE_MYSQL_SCRIPT As String = Path.Combine(SCRIPT_DIR, "create_webgoatcoins.sql")
        Public Shared ReadOnly DB_CREATE_SQLITE_SCRIPT As String = Path.Combine(SCRIPT_DIR, "create_webgoatcoins_sqlite3.sql")
        Public Shared ReadOnly DB_LOAD_MYSQL_SCRIPT As String = Path.Combine(SCRIPT_DIR, "load_webgoatcoins.sql")
        Public Shared ReadOnly DB_LOAD_SQLITE_SCRIPT As String = Path.Combine(SCRIPT_DIR, "load_webgoatcoins_sqlite3.sql")
    End Class
End Namespace