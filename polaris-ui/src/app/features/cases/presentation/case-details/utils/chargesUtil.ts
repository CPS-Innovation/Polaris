import { CustodyTimeLimit } from "../../../domain/gateway/CaseDetails";
import {
  CommonDateTimeFormats,
  formatDate,
} from "../../../../../common/utils/dates";
export const getFormattedCustodyTimeData = (
  custodyTimeLimit: CustodyTimeLimit
) => {
  if (!custodyTimeLimit) {
    return {
      custodyExpiryDays: "N/A",
      custodyExpiryDate: "N/A",
    };
  }
  const { expiryDate, expiryDays, expiryIndicator } = custodyTimeLimit;

  switch (expiryIndicator) {
    case "ACTIVE":
      return {
        custodyExpiryDays: `${expiryDays} ${expiryDays > 1 ? "Days" : "Day"}`,
        custodyExpiryDate: formatDate(
          expiryDate,
          CommonDateTimeFormats.ShortDateTextMonth
        ),
      };

    case "EXPIRED":
      return {
        custodyExpiryDays: "Expired",
        custodyExpiryDate: formatDate(
          expiryDate,
          CommonDateTimeFormats.ShortDateTextMonth
        ),
      };
    default:
      return {
        custodyExpiryDays: "N/A",
        custodyExpiryDate: "N/A",
      };
  }
};
