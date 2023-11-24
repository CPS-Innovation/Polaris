import { AreaDivision } from "../../app/features/cases/domain/redactionLog/AreaDivision";
import { ListItem } from "../../app/features/cases/domain/redactionLog/ListItem";

const areasStub: ListItem[] = [
  {
    id: "1",
    name: AreaDivision.Wales,
    children: [
      {
        id: "1",
        name: "Magistrates Court",
        children: [],
      },
      {
        id: "2",
        name: "Crown Court",
        children: [],
      },
      {
        id: "3",
        name: "RASSO",
        children: [],
      },
      {
        id: "4",
        name: "CCU",
        children: [],
      },
    ],
  },
  {
    id: "2",
    name: AreaDivision.EastMidlands,
    children: [
      {
        id: "1",
        name: "Magistrates Court",
        children: [],
      },
      {
        id: "2",
        name: "Crown Court",
        children: [],
      },
      {
        id: "3",
        name: "RASSO",
        children: [],
      },
      {
        id: "4",
        name: "CCU",
        children: [],
      },
    ],
  },
  {
    id: "3",
    name: AreaDivision.EastOfEngland,
    children: [
      {
        id: "1",
        name: "Magistrates Court",
        children: [],
      },
      {
        id: "2",
        name: "Crown Court",
        children: [],
      },
      {
        id: "3",
        name: "RASSO",
        children: [],
      },
      {
        id: "4",
        name: "CCU",
        children: [],
      },
    ],
  },
  {
    id: "4",
    name: AreaDivision.LondonNorth,
    children: [
      {
        id: "1",
        name: "Magistrates Court",
        children: [],
      },
      {
        id: "2",
        name: "Crown Court",
        children: [],
      },
      {
        id: "3",
        name: "RASSO",
        children: [],
      },
      {
        id: "4",
        name: "CCU",
        children: [],
      },
    ],
  },
  {
    id: "5",
    name: AreaDivision.LondonSouth,
    children: [
      {
        id: "1",
        name: "Magistrates Court",
        children: [],
      },
      {
        id: "2",
        name: "Crown Court",
        children: [],
      },
      {
        id: "3",
        name: "RASSO",
        children: [],
      },
      {
        id: "4",
        name: "CCU",
        children: [],
      },
    ],
  },
  {
    id: "6",
    name: AreaDivision.MerseryCheshire,
    children: [
      {
        id: "1",
        name: "Magistrates Court",
        children: [],
      },
      {
        id: "2",
        name: "Crown Court",
        children: [],
      },
      {
        id: "3",
        name: "RASSO",
        children: [],
      },
      {
        id: "4",
        name: "CCU",
        children: [],
      },
    ],
  },
  {
    id: "7",
    name: AreaDivision.NorthEast,
    children: [
      {
        id: "1",
        name: "Magistrates Court",
        children: [],
      },
      {
        id: "2",
        name: "Crown Court",
        children: [],
      },
      {
        id: "3",
        name: "RASSO",
        children: [],
      },
      {
        id: "4",
        name: "CCU",
        children: [],
      },
    ],
  },
  {
    id: "8",
    name: AreaDivision.NorthWest,
    children: [
      {
        id: "1",
        name: "Magistrates Court",
        children: [],
      },
      {
        id: "2",
        name: "Crown Court",
        children: [],
      },
      {
        id: "3",
        name: "RASSO",
        children: [],
      },
      {
        id: "4",
        name: "CCU",
        children: [],
      },
    ],
  },
  {
    id: "9",
    name: AreaDivision.SouthEast,
    children: [
      {
        id: "1",
        name: "Magistrates Court",
        children: [],
      },
      {
        id: "2",
        name: "Crown Court",
        children: [],
      },
      {
        id: "3",
        name: "RASSO",
        children: [],
      },
      {
        id: "4",
        name: "CCU",
        children: [],
      },
    ],
  },
  {
    id: "10",
    name: AreaDivision.SouthWest,
    children: [
      {
        id: "1",
        name: "Magistrates Court",
        children: [],
      },
      {
        id: "2",
        name: "Crown Court",
        children: [],
      },
      {
        id: "3",
        name: "RASSO",
        children: [],
      },
      {
        id: "4",
        name: "CCU",
        children: [],
      },
    ],
  },
  {
    id: "11",
    name: AreaDivision.ThamesAndChiltern,
    children: [
      {
        id: "1",
        name: "Magistrates Court",
        children: [],
      },
      {
        id: "2",
        name: "Crown Court",
        children: [],
      },
      {
        id: "3",
        name: "RASSO",
        children: [],
      },
      {
        id: "4",
        name: "CCU",
        children: [],
      },
    ],
  },
  {
    id: "12",
    name: AreaDivision.Wessex,
    children: [
      {
        id: "1",
        name: "Magistrates Court",
        children: [],
      },
      {
        id: "2",
        name: "Crown Court",
        children: [],
      },
      {
        id: "3",
        name: "RASSO",
        children: [],
      },
      {
        id: "4",
        name: "CCU",
        children: [],
      },
    ],
  },
  {
    id: "13",
    name: AreaDivision.WestMidlands,
    children: [
      {
        id: "1",
        name: "Magistrates Court",
        children: [],
      },
      {
        id: "2",
        name: "Crown Court",
        children: [],
      },
      {
        id: "3",
        name: "RASSO",
        children: [],
      },
      {
        id: "4",
        name: "CCU",
        children: [],
      },
    ],
  },
  {
    id: "14",
    name: AreaDivision.YorkshireAndHumberside,
    children: [
      {
        id: "1",
        name: "Magistrates Court",
        children: [],
      },
      {
        id: "2",
        name: "Crown Court",
        children: [],
      },
      {
        id: "3",
        name: "RASSO",
        children: [],
      },
      {
        id: "4",
        name: "CCU",
        children: [],
      },
    ],
  },
  {
    id: "96",
    name: AreaDivision.TestArea1,
    children: [
      {
        id: "1",
        name: "Magistrates Court",
        children: [],
      },
      {
        id: "2",
        name: "Crown Court",
        children: [],
      },
      {
        id: "3",
        name: "RASSO",
        children: [],
      },
      {
        id: "4",
        name: "CCU",
        children: [],
      },
    ],
  },
  {
    id: "97",
    name: AreaDivision.TestArea2,
    children: [
      {
        id: "1",
        name: "Magistrates Court",
        children: [],
      },
      {
        id: "2",
        name: "Crown Court",
        children: [],
      },
      {
        id: "3",
        name: "RASSO",
        children: [],
      },
    ],
  },
];

const divisionsStub: ListItem[] = [
  {
    id: "16",
    name: AreaDivision.SpecialCrimeAndCounterTerrorismDivision,
    children: [
      {
        id: "1",
        name: "Appeals and Review Unit",
        children: [],
      },
      {
        id: "2",
        name: "Counter Terrorism",
        children: [],
      },
      {
        id: "3",
        name: "SC London",
        children: [],
      },
      {
        id: "4",
        name: "SC York",
        children: [],
      },
    ],
  },
  {
    id: "17",
    name: AreaDivision.ProceedsOfCrime,
    children: [
      {
        id: "1",
        name: "ARIS",
        children: [],
      },
      {
        id: "2",
        name: "Confiscation",
        children: [],
      },
      {
        id: "3",
        name: "Enforcement",
        children: [],
      },
    ],
  },
  {
    id: "19",
    name: AreaDivision.SEOCIDInternationalLondonAndSouthEastDivision,
    children: [
      {
        id: "1",
        name: "SEOC Team",
        children: [],
      },
      {
        id: "2",
        name: "International",
        children: [],
      },
      {
        id: "3",
        name: "Extradition",
        children: [],
      },
    ],
  },
  {
    id: "20",
    name: AreaDivision.SEOCIDRegionalAndWalesDivision,
    children: [
      {
        id: "1",
        name: "RWD North Unit",
        children: [],
      },
      {
        id: "2",
        name: "RWD Midlands Wales Unit",
        children: [],
      },
      {
        id: "3",
        name: "RWD Management Team",
        children: [],
      },
      {
        id: "4",
        name: "OCSAU",
        children: [],
      },
    ],
  },
  {
    id: "98",
    name: AreaDivision.TestDivision1,
    children: [
      {
        id: "1",
        name: "Magistrates Court",
        children: [],
      },
      {
        id: "2",
        name: "Crown Court",
        children: [],
      },
      {
        id: "3",
        name: "RASSO",
        children: [],
      },
      {
        id: "4",
        name: "CCU",
        children: [],
      },
    ],
  },
  {
    id: "99",
    name: AreaDivision.TestDivision2,
    children: [
      {
        id: "1",
        name: "Magistrates Court",
        children: [],
      },
      {
        id: "2",
        name: "Crown Court",
        children: [],
      },
      {
        id: "3",
        name: "RASSO",
        children: [],
      },
      {
        id: "4",
        name: "CCU",
        children: [],
      },
    ],
  },
];

const documentTypesStub: ListItem[] = [
  { id: "1", name: "MG 0", children: [] },
  { id: "2", name: "MG 1", children: [] },
  { id: "3", name: "MG 2", children: [] },
  { id: "4", name: "MG 3", children: [] },
  { id: "5", name: "MG 4", children: [] },
  { id: "6", name: "MG 4A", children: [] },
  { id: "7", name: "MG 4B", children: [] },
  { id: "8", name: "MG 4C", children: [] },
  { id: "9", name: "MG 4D", children: [] },
  { id: "10", name: "MG 4E", children: [] },
  { id: "11", name: "MG 4F", children: [] },
  { id: "12", name: "MG 5", children: [] },
  { id: "13", name: "MG 6", children: [] },
  { id: "14", name: "MG 6A", children: [] },
  { id: "15", name: "MG 6B", children: [] },
  { id: "16", name: "MG 6C", children: [] },
  { id: "17", name: "MG 6D", children: [] },
  { id: "18", name: "MG 6E", children: [] },
  { id: "19", name: "MG 7", children: [] },
  { id: "20", name: "MG 8", children: [] },
  { id: "21", name: "MG 9", children: [] },
  { id: "22", name: "MG 10", children: [] },
  { id: "23", name: "MG 11", children: [] },
  // beware non-sequential numbering due to late addition of items
  { id: "36", name: "MG 11 Backsheet", children: [] },

  { id: "24", name: "MG 12", children: [] },
  { id: "25", name: "MG 14", children: [] },
  { id: "26", name: "MG 15", children: [] },
  { id: "27", name: "MG 16", children: [] },
  { id: "28", name: "MG 18", children: [] },
  { id: "29", name: "MG 19", children: [] },
  { id: "30", name: "MG 20", children: [] },
  { id: "31", name: "MG 21", children: [] },
  { id: "32", name: "MG 21A", children: [] },
  { id: "33", name: "MG 22", children: [] },
  { id: "34", name: "PNC Print", children: [] },

  // beware non-sequential numbering due to late addition of items
  { id: "37", name: "Exhibits", children: [] },
  { id: "38", name: "Unused", children: [] },
  { id: "39", name: "MME", children: [] },
  { id: "35", name: "Other", children: [] },
];

const investigatingAgenciesStub: ListItem[] = [
  { id: "1", name: "DWP", children: [] },
  { id: "2", name: "NHS", children: [] },
  { id: "3", name: "Counter Fraud Authority", children: [] },
  { id: "4", name: "Department for Health and Social Care", children: [] },
  {
    id: "5",
    name: "Medicines Healthcare Regulatory Agency (MHRA)",
    children: [],
  },
  { id: "6", name: "Welsh Revenue Authority", children: [] },
  { id: "7", name: "Child Maintenance Group", children: [] },
  { id: "8", name: "DEFRA-linked agencies including:", children: [] },
  { id: "9", name: "The Veterinary Medicines Directorate", children: [] },
  {
    id: "10",
    name: "Animal Health and Veterinary Laboratories Agency",
    children: [],
  },
  { id: "11", name: "Forestry Commission Wales", children: [] },
  { id: "12", name: "Rural Payments Agency", children: [] },
  { id: "13", name: "Horticultural Marketing Inspectorate", children: [] },
  { id: "14", name: "Marine Management Organisation", children: [] },
  { id: "15", name: "Gangmasters and Labour Abuse Authority", children: [] },
  { id: "16", name: "British Cattle Movement Service", children: [] },
  { id: "17", name: "Food Standards Agency", children: [] },
  { id: "18", name: "NATIS", children: [] },
  { id: "19", name: "MOD", children: [] },
  { id: "20", name: "NCA", children: [] },
  { id: "21", name: "Local Authority", children: [] },
  {
    id: "22",
    name: "Independent Office for Police Conduct (IOPC)",
    children: [],
  },
  { id: "23", name: "Health and Safety Executive (HSE)", children: [] },
  { id: "24", name: "Home Office Immigration", children: [] },
  { id: "25", name: "HMRC", children: [] },
  { id: "26", name: "ROCU", children: [] },
  { id: "27", name: "SO15", children: [] },
  { id: "28", name: "Police Scotland", children: [] },
  { id: "29", name: "Counter Terrorism Police", children: [] },
  { id: "30", name: "Avon and Somerset Constabulary", children: [] },
  { id: "31", name: "Bedfordshire Police", children: [] },
  { id: "32", name: "Cambridgeshire Constabulary", children: [] },
  { id: "33", name: "Cheshire Constabulary", children: [] },
  { id: "34", name: "City of London Police", children: [] },
  { id: "35", name: "Cleveland Police", children: [] },
  { id: "36", name: "Cumbria Constabulary", children: [] },
  { id: "37", name: "Derbyshire Constabulary", children: [] },
  { id: "38", name: "Devon & Cornwall Police", children: [] },
  { id: "39", name: "Dorset Police", children: [] },
  { id: "40", name: "Durham Constabulary", children: [] },
  { id: "41", name: "Essex Police", children: [] },
  { id: "42", name: "Gloucestershire Constabulary", children: [] },
  { id: "43", name: "Greater Manchester Police", children: [] },
  { id: "44", name: "Hampshire Constabulary", children: [] },
  { id: "45", name: "Hertfordshire Constabulary", children: [] },
  { id: "46", name: "Humberside Police", children: [] },
  { id: "47", name: "Kent Police", children: [] },
  { id: "48", name: "Lancashire Constabulary", children: [] },
  { id: "49", name: "Leicestershire Police", children: [] },
  { id: "50", name: "Lincolnshire Police", children: [] },
  { id: "51", name: "Merseyside Police", children: [] },
  { id: "52", name: "Metropolitan Police Service", children: [] },
  { id: "53", name: "Norfolk Constabulary", children: [] },
  { id: "54", name: "North Yorkshire Police", children: [] },
  { id: "55", name: "Northamptonshire Police", children: [] },
  { id: "56", name: "Northumbria Police", children: [] },
  { id: "57", name: "Nottinghamshire Police", children: [] },
  { id: "58", name: "South Yorkshire Police", children: [] },
  { id: "59", name: "Staffordshire Police", children: [] },
  { id: "60", name: "Suffolk Constabulary", children: [] },
  { id: "61", name: "Surrey Police", children: [] },
  { id: "62", name: "Sussex Police", children: [] },
  { id: "63", name: "Thames Valley Police", children: [] },
  { id: "64", name: "Warwickshire Police", children: [] },
  { id: "65", name: "West Mercia Police", children: [] },
  { id: "66", name: "West Midlands Police", children: [] },
  { id: "67", name: "West Yorkshire Police", children: [] },
  { id: "68", name: "Wiltshire Police", children: [] },
  { id: "69", name: "Dyfed-Powys Police", children: [] },
  { id: "70", name: "Gwent Police", children: [] },
  { id: "71", name: "North Wales Police", children: [] },
  { id: "72", name: "South Wales Police", children: [] },
  { id: "73", name: "British Transport Police", children: [] },
  { id: "74", name: "Civil Nuclear Constabulary", children: [] },
  { id: "75", name: "Ministry of Defence Police", children: [] },
  { id: "76", name: "National Police Air Service", children: [] },
  { id: "77", name: "SEROCU", children: [] },
  { id: "78", name: "ERSOU", children: [] },
];

export const missedRedactionsStub: ListItem[] = [
  { id: "1", name: "Named individual", children: [] },
  { id: "2", name: "Title", children: [] },
  { id: "3", name: "Occupation", children: [] },
  { id: "4", name: "Relationship to others", children: [] },
  { id: "5", name: "Address", children: [] },
  { id: "6", name: "Location", children: [] },
  { id: "7", name: "Vehicle registration", children: [] },
  { id: "8", name: "NHS number", children: [] },
  { id: "9", name: "Date of birth", children: [] },
  { id: "10", name: "Bank details", children: [] },
  { id: "11", name: "NI Number", children: [] },
  { id: "12", name: "Phone number", children: [] },
  { id: "13", name: "Email address", children: [] },
  { id: "14", name: "Previous convictions", children: [] },
  { id: "15", name: "Other", children: [] },
];

export const redactionLogInitialData = {
  areas: areasStub,
  division: divisionsStub,
  documentTypes: documentTypesStub,
  investigatingAgencies: investigatingAgenciesStub,
  missedRedactions: missedRedactionsStub,
};
