/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

using System.Reflection;
using System.Runtime.CompilerServices;
using static NatML.Internal.NatML;

// Metadata
[assembly: AssemblyCompany(@"NatML Inc.")]
[assembly: AssemblyTitle(@"NatML.Runtime")]
[assembly: AssemblyVersionAttribute(Version)]
[assembly: AssemblyCopyright(@"Copyright © 2023 NatML Inc. All Rights Reserved.")]

// Friends
[assembly: InternalsVisibleTo(@"NatML.Editor")]
[assembly: InternalsVisibleTo(@"NatML.Tests")]

// Remove these ASAP
[assembly: InternalsVisibleTo(@"VideoKit")]
[assembly: InternalsVisibleTo(@"VideoKit.Runtime")]
[assembly: InternalsVisibleTo(@"VideoKit.Editor")]