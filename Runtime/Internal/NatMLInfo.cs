/* 
*   NatML
*   Copyright © 2023 NatML Inc. All rights reserved.
*/

using System.Reflection;
using System.Runtime.CompilerServices;
using static NatML.Internal.NatML;

// Metadata
[assembly: AssemblyCompany(@"NatML Inc.")]
[assembly: AssemblyTitle(@"NatML")]
[assembly: AssemblyVersionAttribute(Version)]
[assembly: AssemblyCopyright(@"Copyright © 2023 NatML Inc. All Rights Reserved.")]

// Friends
[assembly: InternalsVisibleTo(@"NatML.ML.Editor")]
[assembly: InternalsVisibleTo(@"NatML.ML.Tests")]