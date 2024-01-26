import { useState, useEffect, useRef } from "react";
import {
  Select,
  TextArea,
  Button,
  Guidance,
  ErrorSummary,
  Spinner,
  LinkButton,
} from "../../../../../common/presentation/components";
import { UnderRedactionContent } from "./UnderRedactionContent";
import { UnderOverRedactionContent } from "./UnderOverRedactionContent";
import { useForm, Controller, FieldErrors } from "react-hook-form";
import { SaveStatus } from "../../../domain/gateway/SaveStatus";
import {
  ChargeStatus,
  ChargeStatusLabels,
} from "../../../domain/redactionLog/ChargeStatus";
import {
  RedactionLogLookUpsData,
  RedactionLogMappingData,
  RedactionTypeData,
} from "../../../domain/redactionLog/RedactionLogData";
import { RedactionCategory } from "../../../domain/redactionLog/RedactionCategory";
import { RedactionLogRequestData } from "../../../domain/redactionLog/RedactionLogRequestData";
import {
  getDefaultValuesFromMappings,
  redactString,
} from "../utils/redactionLogUtils";
import { UnderRedactionFormData } from "../../../domain/redactionLog/RedactionLogFormData";
import { RedactionLogTypes } from "../../../domain/redactionLog/RedactionLogTypes";
import { ReactComponent as WhiteTickIcon } from "../../../../../common/presentation/svgs/whiteTick.svg";
import { useAppInsightsTrackEvent } from "../../../../../common/hooks/useAppInsightsTracks";
import { ReactComponent as DocIcon } from "../../../../../common/presentation/svgs/doc.svg";
import classes from "./RedactionLogContent.module.scss";

type RedactionLogContentProps = {
  caseUrn: string;
  isCaseCharged: boolean;
  owningUnit: string;
  documentName: string;
  cmsDocumentTypeId: number;
  additionalData: {
    documentId: string;
    documentType: string;
    fileCreatedDate: string;
    originalFileName: string;
  };
  redactionLogType: RedactionLogTypes;
  savedRedactionTypes: RedactionTypeData[];
  saveStatus: SaveStatus;
  redactionLogLookUpsData: RedactionLogLookUpsData;
  redactionLogMappingsData: RedactionLogMappingData | null;
  saveRedactionLog: (data: RedactionLogRequestData) => void;
  message?: string;
  handleCloseRedactionLog?: () => void;
};

const NOTES_MAX_CHARACTERS = 400;

export const RedactionLogContent: React.FC<RedactionLogContentProps> = ({
  caseUrn,
  isCaseCharged,
  owningUnit,
  documentName,
  cmsDocumentTypeId,
  additionalData,
  saveStatus,
  redactionLogType,
  saveRedactionLog,
  savedRedactionTypes,
  redactionLogLookUpsData,
  redactionLogMappingsData,
  handleCloseRedactionLog,
}) => {
  const errorSummaryRef = useRef(null);
  const trackEvent = useAppInsightsTrackEvent();
  const [savingRedactionLog, setSavingRedactionLog] = useState(false);
  const [defaultValues, setDefaultValues] = useState<UnderRedactionFormData>({
    cpsArea: "",
    businessUnit: "",
    investigatingAgency: "",
    documentType: "",
    chargeStatus: isCaseCharged
      ? `${ChargeStatus.PostCharge}`
      : `${ChargeStatus.PreCharge}`,
    notes: "",
    returnToIA: "true",
  });
  useEffect(() => {
    if (redactionLogMappingsData) {
      const values = getDefaultValuesFromMappings(
        redactionLogMappingsData,
        redactionLogLookUpsData.ouCodeMapping,
        owningUnit,
        cmsDocumentTypeId,
        caseUrn
      );

      setDefaultValues((defaultValues: any) => ({
        ...defaultValues,
        ...values,
      }));
    }
  }, [
    redactionLogMappingsData,
    owningUnit,
    caseUrn,
    cmsDocumentTypeId,
    redactionLogLookUpsData,
  ]);

  const {
    handleSubmit,
    formState: { errors, isSubmitted },
    register,
    control,
    watch,
    reset,
    getValues,
    trigger,
  } = useForm({ defaultValues, shouldFocusError: false });

  useEffect(() => {
    reset(defaultValues);
  }, [defaultValues, reset]);

  useEffect(() => {
    if (errorSummaryRef.current) {
      (errorSummaryRef?.current as HTMLButtonElement).focus();
    }
  }, [isSubmitted]);

  const [cpsArea] = watch(["cpsArea"]);

  const getMappedSelectItems = () => {
    const areaOrDivisions = [
      ...redactionLogLookUpsData.areas,
      ...redactionLogLookUpsData.divisions,
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
      redactionLogLookUpsData.investigatingAgencies.map((item) => ({
        value: item.id,
        children: item.name,
      }));
    const mappedDocumentTypes = redactionLogLookUpsData.documentTypes.map(
      (item) => ({
        value: item.id,
        children: item.name,
      })
    );

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
          <li>
            {`Supporting notes optional - ${NOTES_MAX_CHARACTERS} characters
            maximum`}
          </li>
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

  const getUnderOrOverRedactionTypesRequestData = (
    formData: UnderRedactionFormData
  ) => {
    let redactions: {
      missedRedaction: RedactionTypeData;
      redactionType: RedactionCategory;
    }[] = [];

    const getFormDataValue = (key: keyof UnderRedactionFormData) => {
      return formData[key];
    };

    const getMissedRedactionType = (id: string) => {
      const docType = redactionLogLookUpsData.missedRedactions.find(
        (type) => type.id === id
      );
      if (docType) {
        return { id: docType.id, name: docType.name };
      }
    };

    const getRedactions = (category: string) => {
      const redactionTypes = Object.keys(formData)
        .filter((key) => key.includes(`${category}-type-`))
        .reduce((arr, key) => {
          const value = getFormDataValue(key);
          if (value) {
            arr.push({
              missedRedaction: getMissedRedactionType(value)!,
              redactionType:
                category === "underRedaction"
                  ? RedactionCategory.UnderRedacted
                  : RedactionCategory.OverRedacted,
              returnedToInvestigativeAuthority:
                category === "underRedaction"
                  ? true
                  : formData.returnToIA === "true",
            });
          }
          return arr;
        }, [] as { missedRedaction: RedactionTypeData; redactionType: RedactionCategory; returnedToInvestigativeAuthority: boolean }[]);
      return redactionTypes;
    };

    if (formData["underRedaction"]) {
      redactions = [...redactions, ...getRedactions("underRedaction")];
    }

    if (formData["overRedaction"]) {
      redactions = [...redactions, ...getRedactions("overRedaction")];
    }

    return redactions;
  };

  const getRedactionLogRequestData = (
    formData: UnderRedactionFormData
  ): RedactionLogRequestData => {
    const areaOrDivisions = [
      ...redactionLogLookUpsData.areas,
      ...redactionLogLookUpsData.divisions,
    ];
    const mappedArea = areaOrDivisions.find(
      (area) => area.id === formData.cpsArea
    )!;
    const mappedBusinessUnit = mappedArea.children.find(
      (businessUnit) => businessUnit.id === formData.businessUnit
    )!;
    const mappedInvestigatingAgency =
      redactionLogLookUpsData.investigatingAgencies.find(
        (investigatingAgency) =>
          investigatingAgency.id === formData.investigatingAgency
      )!;

    const mappedDocumentType = redactionLogLookUpsData.documentTypes.find(
      (documentType) => documentType.id === formData.documentType
    )!;

    let redactions: any[] = [];
    if (redactionLogType === RedactionLogTypes.UNDER) {
      redactions = savedRedactionTypes.map((missedRedaction) => ({
        missedRedaction,
        redactionType: RedactionCategory.UnderRedacted,
        returnedToInvestigativeAuthority: false,
      }));
    } else if (redactionLogType === RedactionLogTypes.UNDER_OVER) {
      redactions = getUnderOrOverRedactionTypesRequestData(formData);
    }

    const mappedData = {
      urn: caseUrn,
      unit: {
        id: `${mappedArea?.id}-${mappedBusinessUnit?.id}`,
        type: "Area" as const,
        areaDivisionName: mappedArea?.name,
        name: mappedBusinessUnit?.name,
      },
      investigatingAgency: {
        id: mappedInvestigatingAgency?.id,
        name: mappedInvestigatingAgency?.name,
      },
      documentType: {
        id: mappedDocumentType?.id,
        name: mappedDocumentType?.name,
      },
      redactions: redactions,
      notes: formData.notes || null,
      chargeStatus: parseInt(formData.chargeStatus) as ChargeStatus,
      cmsValues: {
        ...additionalData,
        documentTypeId: cmsDocumentTypeId,
        originalFileName: redactString(additionalData.originalFileName),
      },
    };

    return mappedData;
  };

  const handleUnderOrOverRedactionTypeError = (name: string) => {
    if (name.includes("underRedaction-type-")) {
      return {
        children: "Select an under-redaction type",
        href: "#checkboxes-under-redaction-types",
        "data-testid": "checkbox-cps-urt-link",
      };
    }
    if (name.includes("overRedaction-type-")) {
      return {
        children: "Select an over-redaction type",
        href: "#checkboxes-over-redaction-types",
        "data-testid": "checkbox-cps-ort-link",
      };
    }
  };

  const errorSummaryProperties = (inputName: string) => {
    switch (inputName) {
      case "cpsArea":
        return {
          children: "Please enter valid CPS Area or Central Casework Division",
          href: "#select-cps-area",
          "data-testid": "select-cps-area-link",
        };
      case "businessUnit":
        return {
          children: "Please enter valid CPS Business Unit",
          href: "#select-cps-bu",
          "data-testid": "select-cps-bu-link",
        };
      case "investigatingAgency":
        return {
          children: "Please enter valid Investigative Agency",
          href: "#select-cps-ia",
          "data-testid": "select-cps-ia-link",
        };
      case "documentType":
        return {
          children: "Please enter valid Document Type",
          href: "#select-cps-dt",
          "data-testid": "select-cps-dt-link",
        };
      case "chargeStatus":
        return {
          children: "Please enter valid Charge Status",
          href: "#select-cps-cs",
          "data-testid": "select-cps-cs-link",
        };

      case "overRedaction":
      case "underRedaction":
        return {
          children: "Select a type of redaction",
          href: "#checkboxes-under-over",
          "data-testid": "checkbox-cps-rt-link",
        };

      default:
        return handleUnderOrOverRedactionTypeError(inputName);
    }
  };

  const getErrorSummaryList = (errors: FieldErrors<UnderRedactionFormData>) => {
    let filteredErrorKeys: string[] = Object.keys(errors);
    if (
      filteredErrorKeys.some((key) => key === "underRedaction") &&
      filteredErrorKeys.some((key) => key === "overRedaction")
    ) {
      filteredErrorKeys = filteredErrorKeys.filter(
        (key) => key !== "underRedaction" && key !== "overRedaction"
      );
      filteredErrorKeys = [...filteredErrorKeys, "underRedaction"];
    }
    if (filteredErrorKeys.some((key) => key.includes("underRedaction-type-"))) {
      filteredErrorKeys = filteredErrorKeys.filter(
        (key) => !key.includes("underRedaction-type-")
      );
      filteredErrorKeys = [...filteredErrorKeys, "underRedaction-type-1"];
    }

    if (filteredErrorKeys.some((key) => key.includes("overRedaction-type-"))) {
      filteredErrorKeys = filteredErrorKeys.filter(
        (key) => !key.includes("overRedaction-type-")
      );
      filteredErrorKeys = [...filteredErrorKeys, "overRedaction-type-1"];
    }

    const errorSummary = filteredErrorKeys.map((error, index) => ({
      reactListKey: `${index}`,
      ...errorSummaryProperties(error)!,
    }));

    return errorSummary;
  };

  const handleAppInsightReporting = (
    newValues: UnderRedactionFormData,
    defaultValues: UnderRedactionFormData,
    redactionLogRequestData: RedactionLogRequestData
  ) => {
    const { notes, ...defaultValuesWithoutNotes } = defaultValues;
    const { notes: newNotes, ...newValuesWithoutNotes } = newValues;
    const hasDefaultValueChange =
      JSON.stringify(defaultValuesWithoutNotes) ===
      JSON.stringify(newValuesWithoutNotes);
    if (!hasDefaultValueChange) {
      trackEvent("Failed Default Mapping Redaction Log", {
        oldValues: defaultValuesWithoutNotes,
        newValues: newValuesWithoutNotes,
      });
    }
    if (redactionLogType === RedactionLogTypes.UNDER) {
      trackEvent("Save Redaction Log", {
        values: redactionLogRequestData,
      });
    }

    if (redactionLogType === RedactionLogTypes.UNDER_OVER) {
      trackEvent("Save Redaction Log Under Over", {
        values: redactionLogRequestData,
      });
    }
  };

  return (
    <div
      className={classes.modalContent}
      data-testid="rl-under-redaction-content"
    >
      {saveStatus === "saving" && (
        <div
          className={classes.savingBanner}
          data-testid="rl-saving-redactions"
        >
          <div className={classes.spinnerWrapper}>
            <Spinner diameterPx={15} ariaLabel={"spinner-animation"} />
          </div>
          <h2 className={classes.bannerText}>Saving redactions...</h2>
        </div>
      )}

      {saveStatus === "saved" && (
        <div className={classes.savedBanner} data-testid="rl-saved-redactions">
          <WhiteTickIcon className={classes.whiteTickIcon} />
          <h2 className={classes.bannerText}>Redactions successfully saved</h2>
        </div>
      )}
      <div className={classes.modalHeadWrapper}>
        <div
          className={`${classes.modalTitleWrapper} ${
            redactionLogType === RedactionLogTypes.UNDER_OVER
              ? classes.modalTitleWrapperTypeOver
              : ""
          }`}
        >
          <h1 className={classes.modalContentHeading}>
            {`${caseUrn}`}
            <span className={classes.greyColor}> - Redaction Log</span>
          </h1>

          <Guidance
            name="Redaction log Guidance"
            className={classes.redactionLogGuidance}
            dataTestId="guidance-redaction-log"
          >
            {redactionLogGuidanceContent()}
          </Guidance>
        </div>
      </div>
      <form
        className={classes.underRedactionForm}
        onSubmit={(event) => {
          handleSubmit(
            (data) => {
              event.preventDefault();
              const redactionLogRequestData = getRedactionLogRequestData(data);
              setSavingRedactionLog(true);
              saveRedactionLog(redactionLogRequestData);
              handleAppInsightReporting(
                data,
                defaultValues,
                redactionLogRequestData
              );
            },
            (errors) => {
              if (errorSummaryRef.current) {
                (errorSummaryRef?.current as HTMLButtonElement).focus();
              }
              console.log("error", errors);
            }
          )(event);
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
          {!!Object.keys(errors).length && (
            <div
              ref={errorSummaryRef}
              tabIndex={-1}
              className={classes.errorSummaryWrapper}
            >
              <ErrorSummary
                data-testid={"redaction-log-error-summary"}
                className={classes.errorSummary}
                errorList={getErrorSummaryList(errors)}
              />
            </div>
          )}

          <section>
            <div className={classes.headingWrapper}>
              <DocIcon className={classes.docIcon} />{" "}
              <h2>
                <span className={classes.greyColor}>
                  Redaction details for:
                </span>
                {`"${documentName}"`}
              </h2>
            </div>

            {redactionLogType === RedactionLogTypes.UNDER && (
              <UnderRedactionContent
                documentName={documentName}
                savedRedactionTypes={savedRedactionTypes}
              />
            )}

            {redactionLogType === RedactionLogTypes.UNDER_OVER && (
              <UnderOverRedactionContent
                redactionTypes={redactionLogLookUpsData.missedRedactions}
                register={register}
                getValues={getValues}
                watch={watch}
                trigger={trigger}
                isSubmitted={isSubmitted}
              />
            )}
          </section>
          <section className={classes.textAreaSection}>
            <Guidance
              name="Guidance on supporting notes"
              className={classes.supportingNotesGuidance}
              dataTestId="guidance-supporting-notes"
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
                    hint={{
                      children: `You can enter up to ${
                        NOTES_MAX_CHARACTERS - field.value.length
                      } characters`,
                    }}
                    maxLength={`${NOTES_MAX_CHARACTERS}`}
                    id="redaction-log-notes"
                    data-testid="redaction-log-notes"
                    label={{
                      children: (
                        <span className={classes.textAreaLabel}>
                          Supporting notes{" "}
                          <span className={classes.greyColor}>(optional)</span>
                        </span>
                      ),
                    }}
                  />
                );
              }}
            />
          </section>
        </div>

        <div className={classes.btnWrapper}>
          <Button
            disabled={saveStatus === "saving" || savingRedactionLog}
            type="submit"
            className={classes.saveBtn}
            data-testid="btn-save-redaction-log"
          >
            Save and Close
          </Button>

          {handleCloseRedactionLog && (
            <LinkButton
              className={classes.cancelBtn}
              onClick={handleCloseRedactionLog}
              dataTestId="btn-redaction-log-cancel"
            >
              Cancel
            </LinkButton>
          )}
        </div>
      </form>
    </div>
  );
};
