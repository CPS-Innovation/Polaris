import { useState } from "react";
import {
  Select,
  TextArea,
  Button,
  Guidance,
} from "../../../../../common/presentation/components";
import { UnderRedactionContent } from "./UnderRedactionContent";
import classes from "./RedactionLogContent.module.scss";
import { useForm, Controller } from "react-hook-form";
import { SaveStatus } from "../../../domain/gateway/SaveStatus";
import {
  ChargeStatus,
  ChargeStatusLabels,
} from "../../../domain/redactionLog/ChargeStatus";
import {
  RedactionLogData,
  RedactionTypeData,
} from "../../../domain/redactionLog/RedactionLogData";
import { RedactionCategory } from "../../../domain/redactionLog/RedactionCategory";
import { RedactionLogRequestData } from "../../../domain/redactionLog/RedactionLogRequestData";
import { AreaDivision } from "../../../domain/redactionLog/AreaDivision";
type RedactionLogContentProps = {
  caseUrn: string;
  documentName: string;
  savedRedactionTypes: RedactionTypeData[];
  saveStatus: SaveStatus;
  redactionLogData: RedactionLogData;
  message?: string;
  saveRedactionLog: (data: RedactionLogRequestData) => void;
};

const defaultValues = {
  cpsArea: "",
  businessUnit: "",
  investigatingAgency: "",
  chargeStatus: `${ChargeStatus.PostCharge}`,
  documentType: "",
  notes: "",
};

export type UnderRedactionFormData = {
  cpsArea: string;
  businessUnit: string;
  investigatingAgency: string;
  chargeStatus: string;
  documentType: string;
  notes: string;
};

export const RedactionLogContent: React.FC<RedactionLogContentProps> = ({
  caseUrn,
  documentName,
  saveStatus,
  saveRedactionLog,
  savedRedactionTypes,
  redactionLogData,
}) => {
  const [savingRedactionLog, setSavingRedactionLog] = useState(false);
  const {
    handleSubmit,
    formState: { errors },
    control,
    watch,
  } = useForm({ defaultValues });

  const [cpsArea] = watch(["cpsArea"]);

  const getMappedSelectItems = () => {
    const areaOrDivisions = [
      ...redactionLogData.areas,
      ...redactionLogData.divisions,
    ];

    const defaultOption = {
      value: "",
      children: "-- Please select --",
      disabled: true,
    };

    const defaultAreaOption = {
      value: "",
      children: "-- Please select --",
      disabled: true,
      businessUnits: [],
    };

    const mappedAreaOrDivisions = areaOrDivisions.map((item) => ({
      value: item.id,
      children: item.name,
      businessUnits: item.children.map((child) => ({
        value: child.id,
        children: child.name,
      })),
    }));
    const mappedInvestigatingAgencies =
      redactionLogData.investigatingAgencies.map((item) => ({
        value: item.id,
        children: item.name,
      }));
    const mappedDocumentTypes = redactionLogData.documentTypes.map((item) => ({
      value: item.id,
      children: item.name,
    }));

    return {
      areaOrDivisions: [defaultAreaOption, ...mappedAreaOrDivisions],
      investigatingAgencies: [defaultOption, ...mappedInvestigatingAgencies],
      documentTypes: [defaultOption, ...mappedDocumentTypes],
    };
  };

  const redactionLogGuidanceContent = () => {
    return (
      <div className={classes.redactionLogGuidanceWrapper}>
        <p className={classes.redactionLogGuidanceTitle}>
          Redaction Log Guidance
        </p>
        <ul className={classes.redactionLogGuidanceList}>
          <li>
            This popup allows the capture of details which will be recorded into
            the Redaction Log automatically
          </li>
          <li>
            Once added, if an entry needs editing or deleting, this should be
            done in the Redaction log
          </li>
          <li>
            Contact with the Investigative Agency or the CPS is not automatic -
            you should contact any such bodies yourself
          </li>
        </ul>
      </div>
    );
  };

  const supportingNotesGuidanceContent = () => {
    return (
      <div className={classes.supportingNotesGuidanceWrapper}>
        <p className={classes.supportingNotesGuidanceTitle}>
          Guidance on supporting notes
        </p>
        <ul className={classes.supportingNotesGuidanceList}>
          <li>
            Detail the redaction issue identified, e.g. Statement of XX
            (Initials) DOB redacted
          </li>
          <li>Avoid recording full names</li>
          <li>Do not record sensitive personal data</li>
          <li>Supporting notes optional - 400 characters maximum</li>
        </ul>
      </div>
    );
  };

  const getMappedBusinessUnits = () => {
    const defaultOption = {
      value: "",
      children: "-- Please select --",
      disabled: true,
    };
    const mappedBusinessUnit = getMappedSelectItems().areaOrDivisions.filter(
      (area) => area.value === cpsArea
    )[0].businessUnits;

    return [defaultOption, ...mappedBusinessUnit];
  };

  const getRedactionLogRequestData = (
    formData: UnderRedactionFormData
  ): RedactionLogRequestData => {
    const areaOrDivisions = [
      ...redactionLogData.areas,
      ...redactionLogData.divisions,
    ];
    const mappedArea = areaOrDivisions.find(
      (area) => area.id === formData.cpsArea
    )!;
    const mappedBusinessUnit = mappedArea.children.find(
      (businessUnit) => businessUnit.id === formData.businessUnit
    )!;
    const mappedInvestigatingAgency =
      redactionLogData.investigatingAgencies.find(
        (investigatingAgency) =>
          investigatingAgency.id === formData.investigatingAgency
      )!;

    const mappedDocumentType = redactionLogData.documentTypes.find(
      (documentType) => documentType.id === formData.documentType
    )!;

    const mappedData = {
      urn: caseUrn,
      unit: {
        id: `${mappedArea?.id}-${mappedBusinessUnit?.id}`,
        type: "Area" as const,
        areaDivisionName: mappedArea?.name as unknown as AreaDivision,
        name: mappedBusinessUnit?.name as unknown as AreaDivision,
      },
      investigatingAgency: {
        id: mappedInvestigatingAgency?.id,
        name: mappedInvestigatingAgency?.name,
      },
      documentType: {
        id: mappedDocumentType?.id,
        name: mappedDocumentType?.name,
      },
      missedRedactions: savedRedactionTypes,
      notes: formData.notes || null,
      returnedToInvestigativeAuthority: false,
      chargeStatus: formData.chargeStatus as unknown as ChargeStatus,
      redactionType: RedactionCategory.UnderRedacted,
    };
    return mappedData;
  };

  return (
    <div className={classes.modalContent}>
      {saveStatus === "saving" && (
        <div className={classes.savingBanner}>
          <span>Saving redactions...</span>
        </div>
      )}

      {saveStatus === "saved" && (
        <div className={classes.savedBanner}>
          <span>Redactions successfully saved</span>
        </div>
      )}
      <div className={classes.modalHeadWrapper}>
        <div className={classes.modalTitleWrapper}>
          <h1 className={classes.modalContentHeading}>
            45AA209820/1 - Redaction Log
          </h1>

          <Guidance
            name="Redaction log Guidance"
            className={classes.redactionLogGuidance}
          >
            {redactionLogGuidanceContent()}
          </Guidance>
        </div>
      </div>
      <form
        className={classes.underRedactionForm}
        onSubmit={() => {
          console.log("hellooo");
        }}
      >
        <div className={classes.selectInputWrapper}>
          <section className={classes.selectSection}>
            <Controller
              name="cpsArea"
              control={control}
              rules={{
                required: true,
              }}
              render={({ field }) => {
                return (
                  <Select
                    {...field}
                    errorMessage={
                      errors.cpsArea && {
                        children: "Select an Area or Division",
                      }
                    }
                    label={{
                      htmlFor: "select-cps-area",
                      children: "CPS Area or Central Casework Division:",
                      className: classes.selectLabel,
                    }}
                    id="select-cps-area"
                    data-testid="select-cps-area"
                    formGroup={{
                      className: classes.select,
                    }}
                    items={getMappedSelectItems().areaOrDivisions}
                  />
                );
              }}
            />
          </section>
          <section className={classes.selectSection}>
            <Controller
              name="businessUnit"
              control={control}
              rules={{
                required: true,
              }}
              render={({ field }) => {
                return (
                  <Select
                    {...field}
                    errorMessage={
                      errors.businessUnit && {
                        children: "Select a Business Unit",
                      }
                    }
                    label={{
                      htmlFor: "select-cps-bu",
                      children: "CPS Business Unit:",
                      className: classes.selectLabel,
                    }}
                    id="select-cps-bu"
                    data-testid="select-cps-bu"
                    formGroup={{
                      className: classes.select,
                    }}
                    items={getMappedBusinessUnits()}
                  />
                );
              }}
            />
          </section>
          <section className={classes.selectSection}>
            <Controller
              name="investigatingAgency"
              control={control}
              rules={{
                required: true,
              }}
              render={({ field }) => {
                return (
                  <Select
                    {...field}
                    errorMessage={
                      errors.investigatingAgency && {
                        children: "Select an Investigative Agency",
                      }
                    }
                    label={{
                      htmlFor: "select-cps-ia",
                      children: "Investigative Agency:",
                      className: classes.selectLabel,
                    }}
                    id="select-cps-ia"
                    data-testid="select-cps-ia"
                    formGroup={{
                      className: classes.select,
                    }}
                    items={getMappedSelectItems().investigatingAgencies}
                  />
                );
              }}
            />
          </section>
          <section className={classes.selectSection}>
            <Controller
              name="chargeStatus"
              control={control}
              rules={{
                required: true,
              }}
              render={({ field }) => {
                return (
                  <Select
                    {...field}
                    errorMessage={
                      errors.chargeStatus && {
                        children: "Select a Charge Status",
                      }
                    }
                    label={{
                      htmlFor: "select-cps-cs",
                      children: "Charge Status:",
                      className: classes.selectLabel,
                    }}
                    id="select-cps-cs"
                    data-testid="select-cps-cs"
                    formGroup={{
                      className: classes.select,
                    }}
                    items={[
                      {
                        children: ChargeStatusLabels[ChargeStatus.PreCharge],
                        value: `${ChargeStatus.PreCharge}`,
                      },
                      {
                        children: ChargeStatusLabels[ChargeStatus.PostCharge],
                        value: `${ChargeStatus.PostCharge}`,
                      },
                    ]}
                  />
                );
              }}
            />
          </section>
          <section className={classes.selectSection}>
            <Controller
              name="documentType"
              control={control}
              rules={{
                required: true,
              }}
              render={({ field }) => {
                return (
                  <Select
                    {...field}
                    errorMessage={
                      errors.documentType && {
                        children: "Select a Document Type",
                      }
                    }
                    label={{
                      htmlFor: "select-cps-dt",
                      children: "Document Type:",
                      className: classes.selectLabel,
                    }}
                    id="select-cps-dt"
                    data-testid="select-cps-dt"
                    formGroup={{
                      className: classes.select,
                    }}
                    items={getMappedSelectItems().documentTypes}
                  />
                );
              }}
            />
          </section>
        </div>

        <div className={classes.modalBodyWrapper}>
          <section>
            <UnderRedactionContent
              documentName={documentName}
              savedRedactionTypes={savedRedactionTypes}
            />
          </section>
          <section className={classes.textAreaSection}>
            <Guidance
              name="Guidance on supporting notes"
              className={classes.supportingNotesGuidance}
            >
              {supportingNotesGuidanceContent()}
            </Guidance>
            <Controller
              name="notes"
              control={control}
              render={({ field }) => {
                return (
                  <TextArea
                    {...field}
                    id="redaction-log-notes"
                    data-testid="redaction-log-notes"
                    label={{
                      children: (
                        <span className={classes.textAreaLabel}>
                          Supporting notes (optional)
                        </span>
                      ),
                    }}
                  />
                );
              }}
            />
          </section>
        </div>
      </form>

      <div className={classes.btnWrapper}>
        <Button
          disabled={saveStatus === "saving" || savingRedactionLog}
          type="submit"
          className={classes.saveBtn}
          onClick={handleSubmit((data) => {
            const redactionLogRequestData = getRedactionLogRequestData({
              ...data,
            });
            setSavingRedactionLog(true);
            saveRedactionLog(redactionLogRequestData);
          })}
          data-testid="btn-save-redaction-log"
        >
          Save and Close
        </Button>
      </div>
    </div>
  );
};
