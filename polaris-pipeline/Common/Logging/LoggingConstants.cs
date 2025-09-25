using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Logging
{
    public class LoggingConstants
    {
        /// <summary>
        /// The log prefix for HSK UI-related log messages.
        /// </summary>
        public const string HskUiLogPrefix = "HSK-UI-BE:";

        /// <summary>
        /// BulkSetUnused operation success log message.
        /// </summary>
        public const string BulkSetUnusedOperationSuccess = "{0} caseId [{1}] successfully reclassified materialId [{2}] as unused";

        /// <summary>
        /// BulkSetUnused operation failed log message.
        /// </summary>
        public const string BulkSetUnusedOperationFailed = "{0} Failed to reclassify materialId [{1}]";

        /// <summary>
        /// UmaReclassify operation failed log message.
        /// </summary>
        public const string UmaReclassifyOperationFailed = "{0} caseId [{1}] UmaReclassify/BulkSetUnused function failed in [{3}]";

        /// <summary>
        /// UmaReclassify operation partial success log message.
        /// </summary>
        public const string UmaReclassifyOperationPartialSuccess = "{0} Milestone: caseId [{1}] UmaReclassify/BulkSetUnused function completed partially successfully in [{2}]";

        /// <summary>
        /// UmaReclassify operation completed log message.
        /// </summary>
        public const string UmaReclassifyOperationCompleted = "{0} Milestone: caseId [{1}] UmaReclassify/BulkSetUnused function completed in [{2}]";

        /// <summary>
        /// RenameMaterial operation success log message.
        /// </summary>
        public const string RenameMaterialOperationSuccess = "{0} caseId [{1}] successfully renamed material name with materialId [{2}]";

        /// <summary>
        /// RenameMaterial operation failed log message.
        /// </summary>
        public const string RenameMaterialOperationFailed = "{0} caseId [{1}] Failed to rename material with materialId [{2}]";

        /// <summary>
        /// User successfully accessed case log message.
        /// </summary>
        public const string UserLogInSuccess = "{0} userName [{1}] name [{2}] successfully accessed caseId [{3}]";

        /// <summary>
        /// UnitName extracted and logged successfully log message.
        /// </summary>
        public const string UnitNameExtractionSuccess = "{0} GetCaseInfo function call returned unitName [{1}] for caseId [{2}]";

        /// <summary>
        /// DiscardMaterial operation success log message.
        /// </summary>
        public const string DiscardMaterialOperationSuccess = "{0} caseId [{1}] successfully discarded material with materialId [{2}]";

        /// <summary>
        /// DiscardMaterial operation failed log message.
        /// </summary>
        public const string DiscardMaterialOperationFailed = "{0} caseId [{1}] Failed to discard material with materialId [{2}]";

        /// <summary>
        /// ReclassifyCaseMaterial operation failed log message.
        /// </summary>
        public const string ReclassifyCaseMaterialOperationFailed = "{0} caseId [{1}] Failed to reclassify material with materialId [{2}]";

        /// <summary>
        /// ReclassifyCaseMaterial operation success log message.
        /// </summary>
        public const string ReclassifyCaseMaterialOperationSuccess = "{0} caseId [{1}] successfully reclassified material with materialId [{2}]";

        /// <summary>
        /// AddWitness operation success log message.
        /// </summary>
        public const string AddWitnessOperationSuccess = "{0} caseId [{1}] successfully added a new witness, witnessId [{2}]";

        /// <summary>
        /// AddWitness operation failed log message.
        /// </summary>
        public const string AddWitnessOperationFailed = "{0} caseId [{1}] failed to add a new witness";

        /// <summary>
        /// AddCaseActionPlan operation success log message.
        /// </summary>
        public const string AddCaseActionPlanOperationSuccess = "{0} Successfully added case action plan to caseId [{1}]";

        /// <summary>
        /// AddCaseActionPlan operation failed log message.
        /// </summary>
        public const string AddCaseActionPlanOperationFailed = "{0} Failed to add case action plan to caseId [{1}]";

        /// <summary>
        /// CaseLockRelease operation success log message.
        /// </summary>
        public const string CaseLockReleaseOperationSuccess = "{0} Successfully released the lock for caseId [{1}]";

        /// <summary>
        /// CaseLockRelease operation failed log message.
        /// </summary>
        public const string CaseLockReleaseOperationFailed = "{0} Failed to release the lock for caseId [{1}]";

        /// <summary>
        /// Update exhibit operation success log message.
        /// </summary>
        public const string UpdateExhibitOperationSuccess = "{0} caseId [{1}] successfully updated exhibit with materialId [{2}]";

        /// <summary>
        /// Update exhibit operation failed.
        /// </summary>
        public const string UpdateExhibitOperationFailed = "{0} caseId [{1}] Failed to update exhibit with materialId [{2}]";

        /// <summary>
        /// Update statement operation success log message.
        /// </summary>
        public const string UpdateStatementOperationSuccess = "{0} caseId [{1}] successfully updated statement with materialId [{2}]";

        /// <summary>
        /// Update statement operation failed.
        /// </summary>
        public const string UpdateStatementOperationFailed = "{0} caseId [{1}] Failed to update statement with materialId [{2}]";
    }
}
