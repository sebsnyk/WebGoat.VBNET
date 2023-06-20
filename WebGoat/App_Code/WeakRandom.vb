Imports System

Namespace OWASP.WebGoat.NET.App_Code
    Public Class WeakRandom
        Private _seed As UInteger = 7

        Public Sub New()
        End Sub

        Public Sub New(seed As UInteger)
            _seed = seed
        End Sub

        Public Function Next(min As UInteger, max As UInteger) As UInteger
            If min >= max Then
                Throw New Exception("Min must be smaller than max")
            End If

            unchecked 'Just use next number from overflow
                _seed = _seed * _seed + _seed
            End unchecked

            Return _seed Mod (max - min) + min
        End Function

        Public Function Peek(min As UInteger, max As UInteger) As UInteger
            If min >= max Then
                Throw New Exception("Min must be smaller than max")
            End If

            unchecked 'Just use next number from overflow
                Dim seed = _seed * _seed + _seed

                Return seed Mod (max - min) + min
            End unchecked
        End Function
    End Class
End Namespace