
Imports System.Collections.Generic
Imports System.Text
Imports System.Runtime.InteropServices

Public Class SetupDiWrap
    Public Shared Function ComPortNameFromFriendlyNamePrefix(friendlyNamePrefix As String, ByRef fulllfriendlyName As String) As String
        Const className As String = "Ports"
        Dim guids As Guid() = GetClassGUIDs(className)

        Dim friendlyNameToComPort As New System.Text.RegularExpressions.Regex(".? \((COM\d+)\)$")
        ' "..... (COMxxx)" -> COMxxxx
        For Each guid As Guid In guids
            ' We start at the "root" of the device tree and look for all
            ' devices that match the interface GUID of a disk
            Dim guidClone As Guid = guid
            Dim h As IntPtr = SetupDiGetClassDevs(guidClone, IntPtr.Zero, IntPtr.Zero, DIGCF_PRESENT Or DIGCF_PROFILE)
            If h.ToInt32() <> INVALID_HANDLE_VALUE Then
                Dim nDevice As Integer = 0
                While True
                    Dim da As New SP_DEVINFO_DATA()
                    da.cbSize = CUInt(Marshal.SizeOf(da))

                    If 0 = SetupDiEnumDeviceInfo(h, nDevice, da) Then
                        Exit While
                    End If
                    nDevice += 1

                    Dim RegType As UInteger
                    Dim ptrBuf As Byte() = New Byte(BUFFER_SIZE - 1) {}
                    Dim RequiredSize As UInteger
                    If SetupDiGetDeviceRegistryProperty(h, da, CUInt(SPDRP.FRIENDLYNAME), RegType, ptrBuf, BUFFER_SIZE,
                        RequiredSize) Then
                        Const utf16terminatorSize_bytes As Integer = 2
                        Dim friendlyName As String = System.Text.UnicodeEncoding.Unicode.GetString(ptrBuf, 0, CInt(RequiredSize) - utf16terminatorSize_bytes)

                        If Not friendlyName.StartsWith(friendlyNamePrefix) Then
                            Continue While
                        End If

                        If Not friendlyNameToComPort.IsMatch(friendlyName) Then
                            Continue While
                        End If

                        fulllfriendlyName = friendlyName
                        Return friendlyNameToComPort.Match(friendlyName).Groups(1).Value
                    End If
                End While
                ' devices
                SetupDiDestroyDeviceInfoList(h)
            End If
        Next
        ' class guids
        Return Nothing
    End Function

    ''' <summary>
    ''' The SP_DEVINFO_DATA structure defines a device instance that is a member of a device information set.
    ''' </summary>
    <StructLayout(LayoutKind.Sequential)>
    Private Structure SP_DEVINFO_DATA
        ''' <summary>Size of the structure, in bytes.</summary>
        Public cbSize As UInteger
        ''' <summary>GUID of the device interface class.</summary>
        Public ClassGuid As Guid
        ''' <summary>Handle to this device instance.</summary>
        Public DevInst As UInteger
        '''' <summary>Reserved; do not use.</summary>
        'public uint Reserved;
        ' WV
        Public Reserved As IntPtr
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Private Structure SP_DEVICE_INTERFACE_DATA
        Public cbSize As Int32
        Public interfaceClassGuid As Guid
        Public flags As Int32
        Private reserved As UIntPtr
    End Structure

    Const BUFFER_SIZE As Integer = 1024

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Auto)>
    Private Structure SP_DEVICE_INTERFACE_DETAIL_DATA
        Public cbSize As Integer
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=BUFFER_SIZE)>
        Public DevicePath As String
    End Structure

    Private Enum SPDRP
        DEVICEDESC = &H0
        HARDWAREID = &H1
        COMPATIBLEIDS = &H2
        NTDEVICEPATHS = &H3
        SERVICE = &H4
        CONFIGURATION = &H5
        CONFIGURATIONVECTOR = &H6
        [CLASS] = &H7
        CLASSGUID = &H8
        DRIVER = &H9
        CONFIGFLAGS = &HA
        MFG = &HB
        FRIENDLYNAME = &HC
        LOCATION_INFORMATION = &HD
        PHYSICAL_DEVICE_OBJECT_NAME = &HE
        CAPABILITIES = &HF
        UI_NUMBER = &H10
        UPPERFILTERS = &H11
        LOWERFILTERS = &H12
        MAXIMUM_PROPERTY = &H13
    End Enum

    <DllImport("setupapi.dll", SetLastError:=True)>
    Private Shared Function SetupDiClassGuidsFromName(ClassName As String, ByRef ClassGuidArray1stItem As Guid, ClassGuidArraySize As UInt32, ByRef RequiredSize As UInt32) As Boolean
    End Function

    <DllImport("setupapi.dll")>
    Friend Shared Function SetupDiGetClassDevsEx(ClassGuid As IntPtr, <MarshalAs(UnmanagedType.LPStr)> enumerator As [String], hwndParent As IntPtr, Flags As Int32, DeviceInfoSet As IntPtr, <MarshalAs(UnmanagedType.LPStr)> MachineName As [String],
            Reserved As IntPtr) As IntPtr
    End Function

    <DllImport("setupapi.dll")>
    Friend Shared Function SetupDiDestroyDeviceInfoList(DeviceInfoSet As IntPtr) As Int32
    End Function

    'ref SP_DEVINFO_DATA devInfo,
    <DllImport("setupapi.dll", CharSet:=CharSet.Auto, SetLastError:=True)>
    Private Shared Function SetupDiEnumDeviceInterfaces(hDevInfo As IntPtr, optionalCrap As IntPtr, ByRef interfaceClassGuid As Guid, memberIndex As UInt32, ByRef deviceInterfaceData As SP_DEVICE_INTERFACE_DATA) As [Boolean]
    End Function

    <DllImport("setupapi.dll")>
    Private Shared Function SetupDiEnumDeviceInfo(DeviceInfoSet As IntPtr, MemberIndex As Int32, ByRef DeviceInterfaceData As SP_DEVINFO_DATA) As Int32
    End Function

    <DllImport("setupapi.dll")>
    Private Shared Function SetupDiClassNameFromGuid(ByRef ClassGuid As Guid, className As StringBuilder, ClassNameSize As Int32, ByRef RequiredSize As Int32) As Int32
    End Function

    <DllImport("setupapi.dll")>
    Private Shared Function SetupDiGetClassDescription(ByRef ClassGuid As Guid, classDescription As StringBuilder, ClassDescriptionSize As Int32, ByRef RequiredSize As Int32) As Int32
    End Function

    <DllImport("setupapi.dll")>
    Private Shared Function SetupDiGetDeviceInstanceId(DeviceInfoSet As IntPtr, ByRef DeviceInfoData As SP_DEVINFO_DATA, DeviceInstanceId As StringBuilder, DeviceInstanceIdSize As Int32, ByRef RequiredSize As Int32) As Int32
    End Function

    ' 1st form using a ClassGUID only, with null Enumerator
    <DllImport("setupapi.dll", CharSet:=CharSet.Auto)>
    Private Shared Function SetupDiGetClassDevs(ByRef ClassGuid As Guid, Enumerator As IntPtr, hwndParent As IntPtr, Flags As Integer) As IntPtr
    End Function

    <DllImport("setupapi.dll", CharSet:=CharSet.Auto, SetLastError:=True)>
    Private Shared Function SetupDiGetDeviceInterfaceDetail(hDevInfo As IntPtr, ByRef deviceInterfaceData As SP_DEVICE_INTERFACE_DATA, ByRef deviceInterfaceDetailData As SP_DEVICE_INTERFACE_DETAIL_DATA, deviceInterfaceDetailDataSize As UInt32, ByRef requiredSize As UInt32, ByRef deviceInfoData As SP_DEVINFO_DATA) As [Boolean]
    End Function

    ' ''' <summary>
    ' ''' The SetupDiGetDeviceRegistryProperty function retrieves the specified device property.
    ' ''' This handle is typically returned by the SetupDiGetClassDevs or SetupDiGetClassDevsEx function.
    ' ''' </summary>
    ' ''' <param Name="DeviceInfoSet">Handle to the device information set that contains the interface and its underlying device.</param>
    ' ''' <param Name="DeviceInfoData">Pointer to an SP_DEVINFO_DATA structure that defines the device instance.</param>
    ' ''' <param Name="Property">Device property to be retrieved. SEE MSDN</param>
    ' ''' <param Name="PropertyRegDataType">Pointer to a variable that receives the registry data Type. This parameter can be NULL.</param>
    ' ''' <param Name="PropertyBuffer">Pointer to a buffer that receives the requested device property.</param>
    ' ''' <param Name="PropertyBufferSize">Size of the buffer, in bytes.</param>
    ' ''' <param Name="RequiredSize">Pointer to a variable that receives the required buffer size, in bytes. This parameter can be NULL.</param>
    ' ''' <returns>If the function succeeds, the return value is nonzero.</returns>
    <DllImport("setupapi.dll", CharSet:=CharSet.Auto, SetLastError:=True)>
    Private Shared Function SetupDiGetDeviceRegistryProperty(DeviceInfoSet As IntPtr, ByRef DeviceInfoData As SP_DEVINFO_DATA, [Property] As UInteger, ByRef PropertyRegDataType As UInt32, PropertyBuffer As Byte(), PropertyBufferSize As UInteger,
            ByRef RequiredSize As UInt32) As Boolean
    End Function


    Const DIGCF_DEFAULT As Integer = &H1
    Const DIGCF_PRESENT As Integer = &H2
    Const DIGCF_ALLCLASSES As Integer = &H4
    Const DIGCF_PROFILE As Integer = &H8
    Const DIGCF_DEVICEINTERFACE As Integer = &H10
    Const INVALID_HANDLE_VALUE As Integer = -1

    Private Shared Function GetClassGUIDs(className As String) As Guid()
        Dim requiredSize As UInt32 = 0
        Dim guidArray As Guid() = New Guid(0) {}

        Dim status As Boolean = SetupDiClassGuidsFromName(className, guidArray(0), 1, requiredSize)
        If True = status Then
            If 1 < requiredSize Then
                guidArray = New Guid(requiredSize - 1) {}
                SetupDiClassGuidsFromName(className, guidArray(0), requiredSize, requiredSize)
            End If
        Else
            Throw New System.ComponentModel.Win32Exception()
        End If

        Return guidArray
    End Function
End Class
