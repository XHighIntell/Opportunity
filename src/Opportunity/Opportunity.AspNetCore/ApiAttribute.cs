namespace Opportunity.AspNetCore;

///<summary>Identifies an action that will automatically map result/exception into container before generate json.</summary>
///<remarks>
/// If successful, output will be like:
/// <code>
/// {
///     "success": true,
///     "result": 📦returned object...
/// }
/// </code>
/// If Excepion, output will be like:
/// <code>
/// {
///     "success": false,
///     "error": {
///         "message": "",
///         "stack": "",
///     }
/// }
/// </code>
///</remarks>
[AttributeUsage(AttributeTargets.Method)]
public class ApiAttribute: Attribute { }

