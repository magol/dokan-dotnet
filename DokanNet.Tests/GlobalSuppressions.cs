using System.Diagnostics.CodeAnalysis;

// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.

/*[assembly:
SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Dokan",
    Scope = "namespace", Target = "DokanNet.Tests")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Dokan")]'*/


[assembly: SuppressMessage("Naming", 
                           "CA1707:Identifiers should not contain underscores", 
                           Justification = "Underscores is used in the names of the UnitTests")]