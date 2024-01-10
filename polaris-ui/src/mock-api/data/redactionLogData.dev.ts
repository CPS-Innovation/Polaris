import { RedactionLogDataSource } from "./types/RedactionLogDataSource";
import { ListItem } from "../../app/features/cases/domain/redactionLog/ListItem";
import { OuCodeMapping } from "../../app/features/cases/domain/redactionLog/RedactionLogData";

const areasStub: ListItem[] = [
  {
    id: "1",
    name: "Cymru/Wales",
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
    name: "East Midlands",
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
    name: "East of England",
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
    name: "London North",
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
    name: "London South",
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
    name: "Mersey-Cheshire",
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
    name: "North East",
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
    name: "North West",
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
    name: "South East",
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
    name: "South West",
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
    name: "Thames and Chiltern",
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
    name: "Wessex",
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
    name: "West Midlands",
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
    name: "Yorkshire and Humberside",
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
    name: "Test Area 1",
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
    name: "Test Area 2",
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
    name: "Special Crime and Counter Terrorism Division",
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
    name: "Proceeds of Crime",
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
    name: "SEOCID International, London and South East Division",
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
    name: "SEOCID Regional and Wales Division",
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
    name: "Test Division 1",
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
    name: "Test Division 2",
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

export const ouCodeMappingStub: OuCodeMapping[] = [
  {
    ouCode: "01",
    areaCode: "4",
    areaName: "London North",
    investigatingAgencyCode: "52",
    investigatingAgencyName: "Metropolitan Police",
  },
  {
    ouCode: "01",
    areaCode: "5",
    areaName: "London South",
    investigatingAgencyCode: "52",
    investigatingAgencyName: "Metropolitan Police",
  },
  {
    ouCode: "03",
    areaCode: "8",
    areaName: "North West",
    investigatingAgencyCode: "36",
    investigatingAgencyName: "Cumbria",
  },
  {
    ouCode: "04",
    areaCode: "8",
    areaName: "North West",
    investigatingAgencyCode: "48",
    investigatingAgencyName: "Lancashire",
  },
  {
    ouCode: "05",
    areaCode: "6",
    areaName: "Mersey/Cheshire",
    investigatingAgencyCode: "51",
    investigatingAgencyName: "Merseyside",
  },
  {
    ouCode: "06",
    areaCode: "8",
    areaName: "North West",
    investigatingAgencyCode: "43",
    investigatingAgencyName: "Greater Manchester",
  },
  {
    ouCode: "07",
    areaCode: "6",
    areaName: "Mersey/Cheshire",
    investigatingAgencyCode: "33",
    investigatingAgencyName: "Cheshire",
  },
  {
    ouCode: "10",
    areaCode: "7",
    areaName: "North East",
    investigatingAgencyCode: "56",
    investigatingAgencyName: "Northumbria",
  },
  {
    ouCode: "11",
    areaCode: "7",
    areaName: "North East",
    investigatingAgencyCode: "40",
    investigatingAgencyName: "Durham",
  },
  {
    ouCode: "12",
    areaCode: "14",
    areaName: "Yorkshire&Humberside",
    investigatingAgencyCode: "54",
    investigatingAgencyName: "North Yorkshire",
  },
  {
    ouCode: "13",
    areaCode: "14",
    areaName: "Yorkshire&Humberside",
    investigatingAgencyCode: "67",
    investigatingAgencyName: "West Yorkshire",
  },
  {
    ouCode: "14",
    areaCode: "14",
    areaName: "Yorkshire&Humberside",
    investigatingAgencyCode: "58",
    investigatingAgencyName: "South Yorkshire",
  },
  {
    ouCode: "16",
    areaCode: "14",
    areaName: "Yorkshire&Humberside",
    investigatingAgencyCode: "46",
    investigatingAgencyName: "Humberside",
  },
  {
    ouCode: "17",
    areaCode: "7",
    areaName: "North East",
    investigatingAgencyCode: "35",
    investigatingAgencyName: "Cleveland",
  },
  {
    ouCode: "20",
    areaCode: "13",
    areaName: "West Midlands",
    investigatingAgencyCode: "66",
    investigatingAgencyName: "West Midlands",
  },
  {
    ouCode: "21",
    areaCode: "13",
    areaName: "West Midlands",
    investigatingAgencyCode: "59",
    investigatingAgencyName: "Staffordshire",
  },
  {
    ouCode: "22",
    areaCode: "13",
    areaName: "West Midlands",
    investigatingAgencyCode: "65",
    investigatingAgencyName: "West Mercia",
  },
  {
    ouCode: "23",
    areaCode: "13",
    areaName: "West Midlands",
    investigatingAgencyCode: "64",
    investigatingAgencyName: "Warwickshire",
  },
  {
    ouCode: "30",
    areaCode: "2",
    areaName: "East Midlands",
    investigatingAgencyCode: "37",
    investigatingAgencyName: "Derbyshire",
  },
  {
    ouCode: "31",
    areaCode: "2",
    areaName: "East Midlands",
    investigatingAgencyCode: "57",
    investigatingAgencyName: "Nottinghamshire",
  },
  {
    ouCode: "32",
    areaCode: "2",
    areaName: "East Midlands",
    investigatingAgencyCode: "50",
    investigatingAgencyName: "Lincolnshire",
  },
  {
    ouCode: "33",
    areaCode: "2",
    areaName: "East Midlands",
    investigatingAgencyCode: "49",
    investigatingAgencyName: "Leicestershire",
  },
  {
    ouCode: "34",
    areaCode: "2",
    areaName: "East Midlands",
    investigatingAgencyCode: "55",
    investigatingAgencyName: "Northamptonshire",
  },
  {
    ouCode: "35",
    areaCode: "3",
    areaName: "East of England",
    investigatingAgencyCode: "32",
    investigatingAgencyName: "Cambridgeshire",
  },
  {
    ouCode: "36",
    areaCode: "3",
    areaName: "East of England",
    investigatingAgencyCode: "53",
    investigatingAgencyName: "Norfolk",
  },
  {
    ouCode: "37",
    areaCode: "3",
    areaName: "East of England",
    investigatingAgencyCode: "60",
    investigatingAgencyName: "Suffolk",
  },
  {
    ouCode: "40",
    areaCode: "11",
    areaName: "Thames&Chiltern",
    investigatingAgencyCode: "31",
    investigatingAgencyName: "Bedfordshire",
  },
  {
    ouCode: "41",
    areaCode: "11",
    areaName: "Thames&Chiltern",
    investigatingAgencyCode: "45",
    investigatingAgencyName: "Hertfordshire",
  },
  {
    ouCode: "42",
    areaCode: "3",
    areaName: "East of England",
    investigatingAgencyCode: "41",
    investigatingAgencyName: "Essex",
  },
  {
    ouCode: "43",
    areaCode: "11",
    areaName: "Thames&Chiltern",
    investigatingAgencyCode: "63",
    investigatingAgencyName: "Thames Valley",
  },
  {
    ouCode: "44",
    areaCode: "12",
    areaName: "Wessex",
    investigatingAgencyCode: "44",
    investigatingAgencyName: "Hampshire & IOW",
  },
  {
    ouCode: "45",
    areaCode: "9",
    areaName: "South East",
    investigatingAgencyCode: "61",
    investigatingAgencyName: "Surrey",
  },
  {
    ouCode: "46",
    areaCode: "9",
    areaName: "South East",
    investigatingAgencyCode: "47",
    investigatingAgencyName: "Kent",
  },
  {
    ouCode: "47",
    areaCode: "9",
    areaName: "South East",
    investigatingAgencyCode: "62",
    investigatingAgencyName: "Sussex",
  },
  {
    ouCode: "48",
    areaCode: "4",
    areaName: "London North",
    investigatingAgencyCode: "34",
    investigatingAgencyName: "City of London",
  },
  {
    ouCode: "48",
    areaCode: "5",
    areaName: "London South",
    investigatingAgencyCode: "34",
    investigatingAgencyName: "City of London",
  },
  {
    ouCode: "50",
    areaCode: "10",
    areaName: "South West",
    investigatingAgencyCode: "38",
    investigatingAgencyName: "Devon and Cornwall",
  },
  {
    ouCode: "52",
    areaCode: "10",
    areaName: "South West",
    investigatingAgencyCode: "30",
    investigatingAgencyName: "Avon & Somerset",
  },
  {
    ouCode: "53",
    areaCode: "10",
    areaName: "South West",
    investigatingAgencyCode: "42",
    investigatingAgencyName: "Gloucestershire",
  },
  {
    ouCode: "54",
    areaCode: "12",
    areaName: "Wessex",
    investigatingAgencyCode: "68",
    investigatingAgencyName: "Wiltshire",
  },
  {
    ouCode: "55",
    areaCode: "12",
    areaName: "Wessex",
    investigatingAgencyCode: "39",
    investigatingAgencyName: "Dorset",
  },
  {
    ouCode: "60",
    areaCode: "1",
    areaName: "Wales",
    investigatingAgencyCode: "71",
    investigatingAgencyName: "North Wales",
  },
  {
    ouCode: "61",
    areaCode: "1",
    areaName: "Wales",
    investigatingAgencyCode: "70",
    investigatingAgencyName: "Gwent",
  },
  {
    ouCode: "62",
    areaCode: "1",
    areaName: "Wales",
    investigatingAgencyCode: "72",
    investigatingAgencyName: "South Wales",
  },
  {
    ouCode: "63",
    areaCode: "1",
    areaName: "Wales",
    investigatingAgencyCode: "69",
    investigatingAgencyName: "Dyfed Powys",
  },
  {
    ouCode: "99",
    areaCode: "96",
    areaName: "Test Area 1",
    investigatingAgencyCode: "43",
    investigatingAgencyName: "Greater Manchester",
  },
];

export const redactionLogLookUpsData = {
  areas: areasStub,
  divisions: divisionsStub,
  documentTypes: documentTypesStub,
  investigatingAgencies: investigatingAgenciesStub,
  missedRedactions: missedRedactionsStub,
  ouCodeMapping: ouCodeMappingStub,
};

const redactionLogMappingData = {
  businessUnits: [
    {
      ou: "Northern CJU (Bristol)",
      areaId: "10",
      unitId: null,
    },
    {
      ou: "Bristol CC",
      areaId: "10",
      unitId: "2",
    },
    {
      ou: "Taunton MC",
      areaId: "10",
      unitId: "1",
    },
    {
      ou: "Taunton CC",
      areaId: "10",
      unitId: "2",
    },
    {
      ou: "BTP Bristol WCU",
      areaId: "10",
      unitId: null,
    },
    {
      ou: "South West RASSO Unit",
      areaId: "10",
      unitId: "3",
    },
    {
      ou: "Bristol",
      areaId: "10",
      unitId: null,
    },
    {
      ou: "North-East",
      areaId: "10",
      unitId: null,
    },
    {
      ou: "Somerset",
      areaId: "10",
      unitId: null,
    },
    {
      ou: "South West CCU",
      areaId: "10",
      unitId: "4",
    },
    {
      ou: "Bristol MC",
      areaId: "10",
      unitId: "1",
    },
    {
      ou: "Avon & Somerset Default WCU",
      areaId: "10",
      unitId: null,
    },
    {
      ou: "Bedfordshire MC",
      areaId: "11",
      unitId: "1",
    },
    {
      ou: "Beds CC",
      areaId: "11",
      unitId: null,
    },
    {
      ou: "Luton WCU (TU)",
      areaId: "11",
      unitId: null,
    },
    {
      ou: "Bedfordshire Default WCU",
      areaId: "11",
      unitId: null,
    },
    {
      ou: "Luton WCU (MCU)",
      areaId: "11",
      unitId: null,
    },
    {
      ou: "Bedford WCU",
      areaId: "11",
      unitId: null,
    },
    {
      ou: "Northwest BTP Area",
      areaId: null,
      unitId: null,
    },
    {
      ou: "HMCPSI",
      areaId: null,
      unitId: null,
    },
    {
      ou: "London South",
      areaId: null,
      unitId: null,
    },
    {
      ou: "London North",
      areaId: null,
      unitId: null,
    },
    {
      ou: "International Division",
      areaId: null,
      unitId: null,
    },
    {
      ou: "SEOCID Int London and SE Div",
      areaId: null,
      unitId: null,
    },
    {
      ou: "SEOCID Regional and Wales Div",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Surrey",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Staffordshire",
      areaId: null,
      unitId: null,
    },
    {
      ou: "West Yorkshire",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Wiltshire",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Gwent",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Durham",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Cheshire",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Cambridgeshire",
      areaId: null,
      unitId: null,
    },
    {
      ou: "West Midlands",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Suffolk",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Thames Valley",
      areaId: null,
      unitId: null,
    },
    {
      ou: "London",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Warwickshire",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Cumbria",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Sussex",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Devon and Cornwall",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Kent",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Derbyshire",
      areaId: null,
      unitId: null,
    },
    {
      ou: "South Wales",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Hampshire & IOW",
      areaId: null,
      unitId: null,
    },
    {
      ou: "North Yorkshire",
      areaId: null,
      unitId: null,
    },
    {
      ou: "North Wales",
      areaId: null,
      unitId: null,
    },
    {
      ou: "LogicaCMG Support Area",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Lancashire",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Hertfordshire",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Dyfed Powys",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Merseyside",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Cleveland",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Northumbria",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Dorset",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Bedfordshire",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Greater Manchester",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Gloucestershire",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Northamptonshire",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Lincolnshire",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Avon & Somerset",
      areaId: null,
      unitId: null,
    },
    {
      ou: "South Yorkshire",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Leicestershire",
      areaId: null,
      unitId: null,
    },
    {
      ou: "West Mercia",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Essex",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Humberside",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Nottinghamshire",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Norfolk",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Proceeds of Crime",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Cambridgeshire Default WCU",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Huntingdon Witness Care Unit",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Southern Witness Care Unit",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Cambridgeshire Magistrates",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Cambridgeshire Crown",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Northern Prosecution Team (Peterborough) - DECOM",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Southern Prosecution Team (Cambridge) - DECOM",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Central Prosecution Team (Huntingdon) - DECOM",
      areaId: null,
      unitId: null,
    },
    {
      ou: "OBSOLETE Cambridgeshire TU",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Cheshire Unity Team",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Eastern Witness Care Unit",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Crewe CCU (Decom)",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Area Secretariat (Decom)",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Crewe MCU (Decom)",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Chester CCU (Decom)",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Chester MCU (Decom)",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Warrington CCU (Decom)",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Warrington MCU (Decom)",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Cheshire Default WCU",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Northern Witness Care Unit",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Western Witness Care Unit",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Cheshire Magistrates Court Unit",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Crewe Business Unit (Decom)",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Cheshire Crown Court Unit",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Cleveland MC",
      areaId: "7",
      unitId: "1",
    },
    {
      ou: "Hartlepool WCU",
      areaId: "7",
      unitId: null,
    },
    {
      ou: "Cleveland Default WCU",
      areaId: "7",
      unitId: null,
    },
    {
      ou: "Middlehaven WCU",
      areaId: "7",
      unitId: null,
    },
    {
      ou: "Teesside WCU (CC)",
      areaId: "7",
      unitId: null,
    },
    {
      ou: "Cleveland CC",
      areaId: "7",
      unitId: "2",
    },
    {
      ou: "Counter Terrorism Leeds",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Counter Terrorism Division WCU",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Counter Terrorism London",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Workington (Decomm)",
      areaId: "8",
      unitId: null,
    },
    {
      ou: "Cumbria Crown Court Hub",
      areaId: "8",
      unitId: "2",
    },
    {
      ou: "Kendal CJU (Decomm)",
      areaId: "8",
      unitId: null,
    },
    {
      ou: "Cumbria Police WCU",
      areaId: "8",
      unitId: null,
    },
    {
      ou: "Cumbria Default WCU",
      areaId: "8",
      unitId: null,
    },
    {
      ou: "Cumbria Magistrates Hub",
      areaId: "8",
      unitId: "1",
    },
    {
      ou: "Kendal TU (Decomm)",
      areaId: "8",
      unitId: null,
    },
    {
      ou: "Derby CC Unit",
      areaId: "2",
      unitId: "2",
    },
    {
      ou: "Derbyshire MC Unit",
      areaId: "2",
      unitId: "1",
    },
    {
      ou: "Do not use - Derbyshire South (Derbyshire) WCU",
      areaId: "2",
      unitId: null,
    },
    {
      ou: "Do not use - Derbyshire Default WCU",
      areaId: "2",
      unitId: null,
    },
    {
      ou: "Derbyshire WCU CC (Do Not Use)",
      areaId: "2",
      unitId: null,
    },
    {
      ou: "Derbyshire WCU",
      areaId: "2",
      unitId: null,
    },
    {
      ou: "Do not use - Derbyshire WCU SOUTH ADHQ",
      areaId: "2",
      unitId: null,
    },
    {
      ou: "Do not use - Derbyshire WCU North",
      areaId: "2",
      unitId: null,
    },
    {
      ou: "IPT Truro",
      areaId: "10",
      unitId: null,
    },
    {
      ou: "Devon and Cornwall CC Unit",
      areaId: "10",
      unitId: "2",
    },
    {
      ou: "Devon and Cornwall Default WCU",
      areaId: "10",
      unitId: null,
    },
    {
      ou: "Exeter WCU",
      areaId: "10",
      unitId: null,
    },
    {
      ou: "Devon and Cornwall MC Unit",
      areaId: "10",
      unitId: "1",
    },
    {
      ou: "Camborne Witness Care Unit",
      areaId: "10",
      unitId: null,
    },
    {
      ou: "Plymouth Witness Care Unit",
      areaId: "10",
      unitId: null,
    },
    {
      ou: "Truro Crown Court Witness Care Unit",
      areaId: "10",
      unitId: null,
    },
    {
      ou: "Exeter Crown Court Witness Care Unit",
      areaId: "10",
      unitId: null,
    },
    {
      ou: "Devon Witness Care Unit",
      areaId: "10",
      unitId: null,
    },
    {
      ou: "Dorset WCU",
      areaId: "12",
      unitId: null,
    },
    {
      ou: "Dorset Magistrates Court Unit",
      areaId: "12",
      unitId: "1",
    },
    {
      ou: "(Decom.) Dorset Magistrates Remand Unit",
      areaId: "12",
      unitId: null,
    },
    {
      ou: "Dorset Default WCU",
      areaId: "12",
      unitId: null,
    },
    {
      ou: "Decomissioned",
      areaId: "12",
      unitId: null,
    },
    {
      ou: "Durham MC",
      areaId: "7",
      unitId: "1",
    },
    {
      ou: "Durham WCU",
      areaId: "7",
      unitId: null,
    },
    {
      ou: "Durham CC",
      areaId: "7",
      unitId: "2",
    },
    {
      ou: "Do not use-Durham Default WCU",
      areaId: "7",
      unitId: null,
    },
    {
      ou: "Dyfed Powys WCU",
      areaId: "1",
      unitId: null,
    },
    {
      ou: "Dyfed Powys MC",
      areaId: "1",
      unitId: "1",
    },
    {
      ou: "Dyfed Powys CC",
      areaId: "1",
      unitId: "2",
    },
    {
      ou: "Dyfed Powys RASSO",
      areaId: "1",
      unitId: null,
    },
    {
      ou: "Dyfed Powys Default WCU",
      areaId: "1",
      unitId: null,
    },
    {
      ou: "Essex TU (Decommissioned)",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Essex Magistrates Court",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Essex Crown Court",
      areaId: null,
      unitId: null,
    },
    {
      ou: "County WCT",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Eastern Group CCU",
      areaId: null,
      unitId: null,
    },
    {
      ou: "East of England RASSO Unit",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Essex Default WCU",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Gloucs CC",
      areaId: "10",
      unitId: "2",
    },
    {
      ou: "Gloucestershire WCU",
      areaId: "10",
      unitId: null,
    },
    {
      ou: "Gloucestershire Default WCU",
      areaId: "10",
      unitId: null,
    },
    {
      ou: "Gloucs MC",
      areaId: "10",
      unitId: "1",
    },
    {
      ou: "Trafford Witness Care Unit",
      areaId: "8",
      unitId: null,
    },
    {
      ou: "Bury WCU",
      areaId: "8",
      unitId: null,
    },
    {
      ou: "Rochdale Witness Care Unit",
      areaId: "8",
      unitId: null,
    },
    {
      ou: "Oldham WCU",
      areaId: "8",
      unitId: null,
    },
    {
      ou: "City of Manchester WCU",
      areaId: "8",
      unitId: null,
    },
    {
      ou: "Wigan WCU",
      areaId: "8",
      unitId: null,
    },
    {
      ou: "Stockport WCU",
      areaId: "8",
      unitId: null,
    },
    {
      ou: "Greater Manchester Default WCU",
      areaId: "8",
      unitId: null,
    },
    {
      ou: "Tameside Witness Care",
      areaId: "8",
      unitId: null,
    },
    {
      ou: "Salford WCU",
      areaId: "8",
      unitId: null,
    },
    {
      ou: "Greater Manchester Magistrates Court Unit",
      areaId: "8",
      unitId: "1",
    },
    {
      ou: "Manchester Complex Case Unit",
      areaId: "8",
      unitId: "4",
    },
    {
      ou: "Bolton WCU",
      areaId: "8",
      unitId: null,
    },
    {
      ou: "Greater Manchester Crown Court Unit",
      areaId: "8",
      unitId: "2",
    },
    {
      ou: "North West Area RASSO Unit",
      areaId: "8",
      unitId: "3",
    },
    {
      ou: "Gwent RASSO",
      areaId: "1",
      unitId: "3",
    },
    {
      ou: "Witness Care Unit (DO NOT USE)",
      areaId: "1",
      unitId: null,
    },
    {
      ou: "Gwent Default WCU",
      areaId: "1",
      unitId: null,
    },
    {
      ou: "Gwent WCU",
      areaId: "1",
      unitId: null,
    },
    {
      ou: "Gwent CC",
      areaId: "1",
      unitId: "2",
    },
    {
      ou: "Gwent MC",
      areaId: "1",
      unitId: "1",
    },
    {
      ou: "Nato",
      areaId: "1",
      unitId: null,
    },
    {
      ou: "Gwent South Unit (decom)",
      areaId: "1",
      unitId: null,
    },
    {
      ou: "Gwent North Unit (decom)",
      areaId: "1",
      unitId: null,
    },
    {
      ou: "HMCPSI Default WCU",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Private Office",
      areaId: null,
      unitId: null,
    },
    {
      ou: "HMCPSI Non Operational WCU",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Wessex Crown Court Unit",
      areaId: "12",
      unitId: "2",
    },
    {
      ou: "Hampshire Magistrates Court Unit",
      areaId: "12",
      unitId: "1",
    },
    {
      ou: "Portsmouth WCU (Decom)",
      areaId: "12",
      unitId: null,
    },
    {
      ou: "Western (Portswood) WCU (Decom)",
      areaId: "12",
      unitId: null,
    },
    {
      ou: "RASSO Eastleigh (Decom)",
      areaId: "12",
      unitId: null,
    },
    {
      ou: "Wessex RASSO Unit",
      areaId: "12",
      unitId: "3",
    },
    {
      ou: "Hampshire CPSD",
      areaId: "12",
      unitId: null,
    },
    {
      ou: "Northern (Basingstoke) WCU (Decom)",
      areaId: "12",
      unitId: null,
    },
    {
      ou: "Hampshire & IOW Default WCU",
      areaId: "12",
      unitId: null,
    },
    {
      ou: "Hampshire WCU",
      areaId: "12",
      unitId: null,
    },
    {
      ou: "Isle of Wight WCU (Decom)",
      areaId: "12",
      unitId: null,
    },
    {
      ou: "Wessex Fraud Unit",
      areaId: "12",
      unitId: null,
    },
    {
      ou: "Wessex Complex Casework Unit",
      areaId: "12",
      unitId: "4",
    },
    {
      ou: "Southampton Witness Care Unit (Decom)",
      areaId: "12",
      unitId: null,
    },
    {
      ou: "WCU West Crown (Decom)",
      areaId: "12",
      unitId: null,
    },
    {
      ou: "Aldershot WCU (Decom)",
      areaId: "12",
      unitId: null,
    },
    {
      ou: "Herts WCU (St Albans TU)",
      areaId: "11",
      unitId: null,
    },
    {
      ou: "Herts WCU East",
      areaId: "11",
      unitId: null,
    },
    {
      ou: "Herts WCU Central and West",
      areaId: "11",
      unitId: null,
    },
    {
      ou: "Herts CC",
      areaId: "11",
      unitId: "2",
    },
    {
      ou: "Hertfordshire MC",
      areaId: "11",
      unitId: "1",
    },
    {
      ou: "Herts CJU Central (Decomissioned)",
      areaId: "11",
      unitId: null,
    },
    {
      ou: "Hertfordshire Default WCU",
      areaId: "11",
      unitId: null,
    },
    {
      ou: "Humberside Default WCU",
      areaId: "14",
      unitId: null,
    },
    {
      ou: "Hull WCU",
      areaId: "14",
      unitId: null,
    },
    {
      ou: "xGrimsby and Scunthorpe Crown Court Unit",
      areaId: "14",
      unitId: null,
    },
    {
      ou: "xGrimsby and Scunthorpe Magistrates Unit",
      areaId: "14",
      unitId: null,
    },
    {
      ou: "Grimsby WCU",
      areaId: "14",
      unitId: null,
    },
    {
      ou: "Humberside Crown Court Unit",
      areaId: "14",
      unitId: "2",
    },
    {
      ou: "Humberside Magistrates Unit",
      areaId: "14",
      unitId: "1",
    },
    {
      ou: "Humberside WCU",
      areaId: "14",
      unitId: null,
    },
    {
      ou: "Humberside and South Yorkshire RASSO unit",
      areaId: "14",
      unitId: "3",
    },
    {
      ou: "International (Decom)",
      areaId: null,
      unitId: null,
    },
    {
      ou: "International WCU",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Kent Crown",
      areaId: "9",
      unitId: "2",
    },
    {
      ou: "Kent Default WCU",
      areaId: "9",
      unitId: null,
    },
    {
      ou: "Kent CPSD",
      areaId: "9",
      unitId: null,
    },
    {
      ou: "Kent RASSO",
      areaId: "9",
      unitId: "3",
    },
    {
      ou: "Kent Mags",
      areaId: "9",
      unitId: "1",
    },
    {
      ou: "Kent VWCU",
      areaId: "9",
      unitId: null,
    },
    {
      ou: "South East Complex Casework Unit",
      areaId: "9",
      unitId: "4",
    },
    {
      ou: "xLancaster Witness Care Unit (Decom)",
      areaId: "8",
      unitId: null,
    },
    {
      ou: "Lancashire Crown Court Unit",
      areaId: "8",
      unitId: "2",
    },
    {
      ou: "Lancashire Police WCU",
      areaId: "8",
      unitId: null,
    },
    {
      ou: "Lancashire Motoring WCU",
      areaId: "8",
      unitId: null,
    },
    {
      ou: "xBlackburn CJU (Decom)",
      areaId: "8",
      unitId: null,
    },
    {
      ou: "xBlackpool CU (Decom)",
      areaId: "8",
      unitId: null,
    },
    {
      ou: "xLancaster CU (Decom)",
      areaId: "8",
      unitId: null,
    },
    {
      ou: "xSouth West Lancs CU (Decom)",
      areaId: "8",
      unitId: null,
    },
    {
      ou: "xPreston CU (Decom)",
      areaId: "8",
      unitId: null,
    },
    {
      ou: "xBlackburn CU (Decom)",
      areaId: "8",
      unitId: null,
    },
    {
      ou: "xBurnley CU (Decom)",
      areaId: "8",
      unitId: null,
    },
    {
      ou: "xBlackpool Witness Care Unit (Decom)",
      areaId: "8",
      unitId: null,
    },
    {
      ou: "Lancashire Default WCU",
      areaId: "8",
      unitId: null,
    },
    {
      ou: "Lancashire/Cumbria Complex Case Unit",
      areaId: "8",
      unitId: "4",
    },
    {
      ou: "North West Area CPS WCU",
      areaId: "8",
      unitId: null,
    },
    {
      ou: "xEast Lancashire Crown Court Unit (DECOM)",
      areaId: "8",
      unitId: null,
    },
    {
      ou: "xBurnley WCU (Decom)",
      areaId: "8",
      unitId: null,
    },
    {
      ou: "Southern Magistrates Unit",
      areaId: "8",
      unitId: "1",
    },
    {
      ou: "Western Magistrates Unit",
      areaId: "8",
      unitId: "1",
    },
    {
      ou: "Lancashire Magistrates Unit",
      areaId: "8",
      unitId: "1",
    },
    {
      ou: "xPreston Witness Care Unit (Decom)",
      areaId: "8",
      unitId: null,
    },
    {
      ou: "xPreston TU (Decom)",
      areaId: "8",
      unitId: null,
    },
    {
      ou: "xPreston CJU (Decom)",
      areaId: "8",
      unitId: null,
    },
    {
      ou: "xSouth West Lancs CJU (Decom)",
      areaId: "8",
      unitId: null,
    },
    {
      ou: "xFylde TU (Decom)",
      areaId: "8",
      unitId: null,
    },
    {
      ou: "xBlackpool CJU (Decom)",
      areaId: "8",
      unitId: null,
    },
    {
      ou: "xLancaster CJU (Decom)",
      areaId: "8",
      unitId: null,
    },
    {
      ou: "xBurnley TU (Decom)",
      areaId: "8",
      unitId: null,
    },
    {
      ou: "xBurnley CJU (Decom)",
      areaId: "8",
      unitId: null,
    },
    {
      ou: "Leicestershire Default WCU",
      areaId: "2",
      unitId: null,
    },
    {
      ou: "Leicestershire WCU",
      areaId: "2",
      unitId: null,
    },
    {
      ou: "Leicestershire MC Unit",
      areaId: "2",
      unitId: "1",
    },
    {
      ou: "Leics CC Unit",
      areaId: "2",
      unitId: "2",
    },
    {
      ou: "Lincolnshire CC",
      areaId: "2",
      unitId: "2",
    },
    {
      ou: "Lincolnshire MC Unit",
      areaId: "2",
      unitId: "1",
    },
    {
      ou: "Lincolnshire Default WCU",
      areaId: "2",
      unitId: null,
    },
    {
      ou: "Lincoln Witness Care Unit",
      areaId: "2",
      unitId: null,
    },
    {
      ou: "LogicaCMG Support Unit",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Test WMC WCU",
      areaId: null,
      unitId: null,
    },
    {
      ou: "London Area Secretariat",
      areaId: null,
      unitId: null,
    },
    {
      ou: "RASSO London (Do not use)",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Tower Hamlets WCU",
      areaId: "4",
      unitId: null,
    },
    {
      ou: "Redbridge WCU",
      areaId: "4",
      unitId: null,
    },
    {
      ou: "Camden and Islington WCU",
      areaId: "4",
      unitId: null,
    },
    {
      ou: "London N CPSD",
      areaId: "4",
      unitId: null,
    },
    {
      ou: "London W CPSD",
      areaId: "4",
      unitId: null,
    },
    {
      ou: "London North-East Crown Court Unit",
      areaId: "4",
      unitId: "2",
    },
    {
      ou: "London North-West Crown Court Unit",
      areaId: "4",
      unitId: "2",
    },
    {
      ou: "London North-East Magistrates Court Unit",
      areaId: "4",
      unitId: "1",
    },
    {
      ou: "London North-West Magistrates Court Unit",
      areaId: "4",
      unitId: "1",
    },
    {
      ou: "London North RASSO Unit",
      areaId: "4",
      unitId: "3",
    },
    {
      ou: "London North RASSO - Confirmed Alignment",
      areaId: "4",
      unitId: "3",
    },
    {
      ou: "London North RASSO - Non-Confirmed Alignment",
      areaId: "4",
      unitId: "3",
    },
    {
      ou: "London North - Confirmed Alignment",
      areaId: "4",
      unitId: null,
    },
    {
      ou: "London North - Non-Confirmed Alignment",
      areaId: "4",
      unitId: null,
    },
    {
      ou: "Homicide Team",
      areaId: "4",
      unitId: "2",
    },
    {
      ou: "Stoke Newington WCU",
      areaId: "4",
      unitId: null,
    },
    {
      ou: "Colindale Police Station WCU",
      areaId: "4",
      unitId: null,
    },
    {
      ou: "Heathrow (SO18) WCU",
      areaId: "4",
      unitId: null,
    },
    {
      ou: "Hillingdon WCU",
      areaId: "4",
      unitId: null,
    },
    {
      ou: "Havering WCU",
      areaId: "4",
      unitId: null,
    },
    {
      ou: "Harrow WCU",
      areaId: "4",
      unitId: null,
    },
    {
      ou: "Brent/Wembley WCU",
      areaId: "4",
      unitId: null,
    },
    {
      ou: "Hounslow Borough WCU",
      areaId: "4",
      unitId: null,
    },
    {
      ou: "Ealing Borough WCU",
      areaId: "4",
      unitId: null,
    },
    {
      ou: "Chingford WCU",
      areaId: "4",
      unitId: null,
    },
    {
      ou: "Newham WCU",
      areaId: "4",
      unitId: null,
    },
    {
      ou: "Haringey WCU",
      areaId: "4",
      unitId: null,
    },
    {
      ou: "Barking and Dagenham WCU",
      areaId: "4",
      unitId: null,
    },
    {
      ou: "Edmonton WCU",
      areaId: "4",
      unitId: null,
    },
    {
      ou: "Westminster WCU",
      areaId: "5",
      unitId: null,
    },
    {
      ou: "Peckham WCU",
      areaId: "5",
      unitId: null,
    },
    {
      ou: "Sutton Witness Care Unit",
      areaId: "5",
      unitId: null,
    },
    {
      ou: "Lambeth WCU",
      areaId: "5",
      unitId: null,
    },
    {
      ou: "Croydon Witness Care Unit",
      areaId: "5",
      unitId: null,
    },
    {
      ou: "Lewisham WCU",
      areaId: "5",
      unitId: null,
    },
    {
      ou: "City of London WCU",
      areaId: "5",
      unitId: null,
    },
    {
      ou: "London South-Southern Crown Court Unit",
      areaId: "5",
      unitId: "2",
    },
    {
      ou: "Southwark WCU",
      areaId: "5",
      unitId: null,
    },
    {
      ou: "Economic Crime Dept",
      areaId: "5",
      unitId: null,
    },
    {
      ou: "London S CPSD",
      areaId: "5",
      unitId: null,
    },
    {
      ou: "Serious Crimes and Operations WCU",
      areaId: "5",
      unitId: null,
    },
    {
      ou: "Roads Policing",
      areaId: "5",
      unitId: null,
    },
    {
      ou: "Professional Standards",
      areaId: "5",
      unitId: null,
    },
    {
      ou: "London South-Central Magistrates Court Unit",
      areaId: "5",
      unitId: "1",
    },
    {
      ou: "London South-Southern Magistrates Court Unit",
      areaId: "5",
      unitId: "1",
    },
    {
      ou: "London South RASSO Unit",
      areaId: "5",
      unitId: "3",
    },
    {
      ou: "London South RASSO - Confirmed Alignment",
      areaId: "5",
      unitId: "3",
    },
    {
      ou: "London South RASSO - Non-Confirmed Alignment",
      areaId: "5",
      unitId: "3",
    },
    {
      ou: "London South - Confirmed Alignment",
      areaId: "5",
      unitId: null,
    },
    {
      ou: "London South - Non-Confirmed Alignment",
      areaId: "5",
      unitId: null,
    },
    {
      ou: "London Complex Casework Unit",
      areaId: "5",
      unitId: "4",
    },
    {
      ou: "Inner London Youth CC",
      areaId: "5",
      unitId: "2",
    },
    {
      ou: "BTP London WCU",
      areaId: "5",
      unitId: null,
    },
    {
      ou: "Wimbledon WCU",
      areaId: "5",
      unitId: null,
    },
    {
      ou: "London South-Central Crown Court Unit",
      areaId: "5",
      unitId: "2",
    },
    {
      ou: "Police Complaints Unit",
      areaId: "5",
      unitId: null,
    },
    {
      ou: "Richmond upon Thames WCU",
      areaId: "5",
      unitId: null,
    },
    {
      ou: "Walworth WCU",
      areaId: "5",
      unitId: null,
    },
    {
      ou: "Wandsworth Borough WCU",
      areaId: "5",
      unitId: null,
    },
    {
      ou: "Hammersmith and Fulham WCU",
      areaId: "5",
      unitId: null,
    },
    {
      ou: "Bromley WCU",
      areaId: "5",
      unitId: null,
    },
    {
      ou: "Kingston WCU",
      areaId: "5",
      unitId: null,
    },
    {
      ou: "Kensington and Chelsea WCU",
      areaId: "5",
      unitId: null,
    },
    {
      ou: "Greenwich WCU",
      areaId: "5",
      unitId: null,
    },
    {
      ou: "London Default WCU",
      areaId: "5",
      unitId: null,
    },
    {
      ou: "Bexley WCU",
      areaId: "5",
      unitId: null,
    },
    {
      ou: "Merseyside Cheshire Fraud Unit",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Merseyside Unity Team",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Liverpool CJU (Decom)",
      areaId: null,
      unitId: null,
    },
    {
      ou: "MN TU (Decom)",
      areaId: null,
      unitId: null,
    },
    {
      ou: "MN CJU (Decom)",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Robbery Unit (Decom)",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Wirral CJU (Decom)",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Merseyside/Cheshire CCU",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Liverpool Contested - Blue Section (Decom)",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Liverpool Contested - Red Section (Decom)",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Liverpool - YES (Decom)",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Mersey North - Knowsley (Decom)",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Mersey North Sefton MCU (DO NOT USE)",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Mersey North K/STH MCU (DO NOT USE)",
      areaId: null,
      unitId: null,
    },
    {
      ou: "North Liverpool CJC (Decom)",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Wirral MCU (DO NOT USE)",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Wirral - YES (Decom)",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Wirral Witness Care Unit",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Liverpool Witness Care Unit",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Knowsley St Helens Witness Care Unit",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Merseyside Default WCU",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Sefton Witness Care Unit",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Merseyside Magistrates Court Unit",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Merseyside CPSD (Decom)",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Merseyside Crown Court Unit",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Merseyside Unity Witness Care Unit",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Mersey CCU (Decom)",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Norfolk Western Area MC",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Norfolk Default WCU",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Norfolk Magistrates Court Team",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Norfolk Eastern Team (DECOMM)",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Norfolk Crown Court Team",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Norwich WCU (MC)",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Norwich WCU (CC)",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Norfolk Eastern Area MC",
      areaId: null,
      unitId: null,
    },
    {
      ou: "North Wales RASSO",
      areaId: "1",
      unitId: "3",
    },
    {
      ou: "North Wales MC",
      areaId: "1",
      unitId: "1",
    },
    {
      ou: "WLU Colwyn Bay",
      areaId: "1",
      unitId: null,
    },
    {
      ou: "Western Unit North Wales",
      areaId: "1",
      unitId: null,
    },
    {
      ou: "North Wales CC",
      areaId: "1",
      unitId: "2",
    },
    {
      ou: "North Wales Default WCU",
      areaId: "1",
      unitId: null,
    },
    {
      ou: "Wrexham TU (Decom)",
      areaId: "1",
      unitId: null,
    },
    {
      ou: "North Wales Area CJU (Decom)",
      areaId: "1",
      unitId: null,
    },
    {
      ou: "WLU Wrexham",
      areaId: "1",
      unitId: null,
    },
    {
      ou: "Central Unit North Wales",
      areaId: "1",
      unitId: null,
    },
    {
      ou: "Eryri TU (Decom)",
      areaId: "1",
      unitId: null,
    },
    {
      ou: "North Yorkshire Default WCU",
      areaId: "14",
      unitId: null,
    },
    {
      ou: "Scarborough combined CJU/TU(Decom)",
      areaId: "14",
      unitId: null,
    },
    {
      ou: "York Combined CJU/TU(Decom)",
      areaId: "14",
      unitId: null,
    },
    {
      ou: "York CJU (Decommissioned)",
      areaId: "14",
      unitId: null,
    },
    {
      ou: "Harrogate Combined CJU/TU(Decom)",
      areaId: "14",
      unitId: null,
    },
    {
      ou: "North Yorkshire WCU",
      areaId: "14",
      unitId: null,
    },
    {
      ou: "North Yorkshire Magistrates Court",
      areaId: "14",
      unitId: "1",
    },
    {
      ou: "North Yorkshire Crown Court",
      areaId: "14",
      unitId: "2",
    },
    {
      ou: "Obsolete Northamptonshire CJU - Do not use",
      areaId: "2",
      unitId: null,
    },
    {
      ou: "Northampton Crown Court Unit",
      areaId: "2",
      unitId: "2",
    },
    {
      ou: "Obsolete Northamptonshire TU - Do not use",
      areaId: "2",
      unitId: null,
    },
    {
      ou: "Northamptonshire North(do not use)",
      areaId: "2",
      unitId: null,
    },
    {
      ou: "Northamptonshire MC Unit",
      areaId: "2",
      unitId: "1",
    },
    {
      ou: "Northamptonshire Default WCU",
      areaId: "2",
      unitId: null,
    },
    {
      ou: "Northamptonshire WCU",
      areaId: "2",
      unitId: null,
    },
    {
      ou: "North East RASSO unit",
      areaId: "7",
      unitId: "3",
    },
    {
      ou: "South Shields Witness Care Unit",
      areaId: "7",
      unitId: null,
    },
    {
      ou: "Gateshead CJU Witness Care Unit",
      areaId: "7",
      unitId: null,
    },
    {
      ou: "Newcastle CJU Witness Care Unit",
      areaId: "7",
      unitId: null,
    },
    {
      ou: "Newcastle CC Witness Care Unit",
      areaId: "7",
      unitId: null,
    },
    {
      ou: "North Shields WCU",
      areaId: "7",
      unitId: null,
    },
    {
      ou: "Northumbria Default WCU",
      areaId: "7",
      unitId: null,
    },
    {
      ou: "Sunderland WCU (CJU)",
      areaId: "7",
      unitId: null,
    },
    {
      ou: "BTP North East WCU",
      areaId: "7",
      unitId: null,
    },
    {
      ou: "Bedlington Witness Care Unit",
      areaId: "7",
      unitId: null,
    },
    {
      ou: "Northumbria MC",
      areaId: "7",
      unitId: "1",
    },
    {
      ou: "North East CCU",
      areaId: "7",
      unitId: "4",
    },
    {
      ou: "Northumbria CC",
      areaId: "7",
      unitId: "2",
    },
    {
      ou: "Northwest BTP WCU",
      areaId: null,
      unitId: null,
    },
    {
      ou: "East Midlands RASSO Unit",
      areaId: "2",
      unitId: "3",
    },
    {
      ou: "Obsolete Nottinghamshire County CU",
      areaId: "2",
      unitId: null,
    },
    {
      ou: "Obsolete Nottinghamshire Youth CU",
      areaId: "2",
      unitId: null,
    },
    {
      ou: "Nottinghamshire Crown Court Unit",
      areaId: "2",
      unitId: "2",
    },
    {
      ou: "Obsolete Nottinghamshire SCIU",
      areaId: "2",
      unitId: null,
    },
    {
      ou: "Nottingham North WCU",
      areaId: "2",
      unitId: null,
    },
    {
      ou: "Nottinghamshire Default WCU",
      areaId: "2",
      unitId: null,
    },
    {
      ou: "Nottingham City WCU",
      areaId: "2",
      unitId: null,
    },
    {
      ou: "Obsolete Nottingham City 2 WCU",
      areaId: "2",
      unitId: null,
    },
    {
      ou: "Obsolete Nottinghamshire County MC Unit",
      areaId: "2",
      unitId: null,
    },
    {
      ou: "Nottinghamshire MC Unit",
      areaId: "2",
      unitId: "1",
    },
    {
      ou: "Notts CPSD",
      areaId: "2",
      unitId: null,
    },
    {
      ou: "East Midlands Complex Casework Unit",
      areaId: "2",
      unitId: "4",
    },
    {
      ou: "Obsolete Nottinghamshire Bridewell CU",
      areaId: "2",
      unitId: null,
    },
    {
      ou: "Obsolete Nottinghamshire Oxclose Carlton CU",
      areaId: "2",
      unitId: null,
    },
    {
      ou: "Organised Crime Division Hacking (Decom)",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Organised Crime Division WCU",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Organised Crime Division Birmingham (Decom)",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Organised Crime Division Calder (Decom)",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Proceeds of Crime Unit Pre-Enforcement",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Organised Crime Division Manchester (Decom)",
      areaId: null,
      unitId: null,
    },
    {
      ou: "UK Border Agency (Decom)",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Proceeds of Crime Unit Enforcement",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Proceeds of Crime Unit Civil Claims",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Proceeds of Crime Unit Civil Recovery",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Extradition",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Pre-Enforcement North",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Regional Asset Recovery Team",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Decommissioned North West Team",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Proceeds of Crime WCU",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Enforcement",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Pre-Enforcement South",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Civil Litigation",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Civil Recovery",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Decommissioned Eastern Team",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Decommissioned East Midlands Team",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Decommissioned London Team",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Decommissioned North East Team",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Decommissioned South East Team",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Decommissioned South West Team",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Decommissioned Wales Team",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Decommissioned West Midlands Team",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Enforcement - ARIS",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Records Management Team (RMT)",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Temporary RMU",
      areaId: null,
      unitId: null,
    },
    {
      ou: "RCD Excise Unit",
      areaId: null,
      unitId: null,
    },
    {
      ou: "RCD Commercial And Policy Unit",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Witness Care Unit RCD - NOT REQUIRED",
      areaId: null,
      unitId: null,
    },
    {
      ou: "RCD Tax Unit",
      areaId: null,
      unitId: null,
    },
    {
      ou: "SEOC Team 1",
      areaId: null,
      unitId: null,
    },
    {
      ou: "SEOC Team 2",
      areaId: null,
      unitId: null,
    },
    {
      ou: "RWD North Unit",
      areaId: null,
      unitId: null,
    },
    {
      ou: "RWD Midlands Wales Unit",
      areaId: null,
      unitId: null,
    },
    {
      ou: "OCSAU",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Cymru Wales Fraud Unit",
      areaId: "1",
      unitId: null,
    },
    {
      ou: "Merthyr MC",
      areaId: "1",
      unitId: "1",
    },
    {
      ou: "Cardiff Merthyr MC",
      areaId: "1",
      unitId: "1",
    },
    {
      ou: "Cardiff Merthyr CC",
      areaId: "1",
      unitId: "2",
    },
    {
      ou: "Swansea MC",
      areaId: "1",
      unitId: "1",
    },
    {
      ou: "Swansea CC",
      areaId: "1",
      unitId: "2",
    },
    {
      ou: "S Wales Area Secretariat (Do Not Use)",
      areaId: "1",
      unitId: null,
    },
    {
      ou: "Cardiff WCU",
      areaId: "1",
      unitId: null,
    },
    {
      ou: "Merthyr Tydfil WCU",
      areaId: "1",
      unitId: null,
    },
    {
      ou: "Pentrebach WCU",
      areaId: "1",
      unitId: null,
    },
    {
      ou: "South Wales Default WCU",
      areaId: "1",
      unitId: null,
    },
    {
      ou: "West Glamorgan WCU",
      areaId: "1",
      unitId: null,
    },
    {
      ou: "Wales Complex Casework Unit",
      areaId: "1",
      unitId: "4",
    },
    {
      ou: "South Wales CPSD",
      areaId: "1",
      unitId: null,
    },
    {
      ou: "Wales RASSO",
      areaId: "1",
      unitId: "3",
    },
    {
      ou: "Rotherham WCU",
      areaId: "14",
      unitId: null,
    },
    {
      ou: "South Yorkshire Magistrates Court Unit",
      areaId: "14",
      unitId: "1",
    },
    {
      ou: "South Yorkshire Crown Court",
      areaId: "14",
      unitId: "2",
    },
    {
      ou: "South Yorks PCPT Unit",
      areaId: "14",
      unitId: null,
    },
    {
      ou: "Sheffield WCU",
      areaId: "14",
      unitId: null,
    },
    {
      ou: "South Yorkshire Default WCU",
      areaId: "14",
      unitId: null,
    },
    {
      ou: "South Yorkshire WCU (TU)",
      areaId: "14",
      unitId: null,
    },
    {
      ou: "Doncaster WCU",
      areaId: "14",
      unitId: null,
    },
    {
      ou: "Barnsley WCU",
      areaId: "14",
      unitId: null,
    },
    {
      ou: "Hillsborough",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Special Crime York",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Special Crime Division WCU",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Special Crime London",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Special Crime Appeals",
      areaId: null,
      unitId: null,
    },
    {
      ou: "London - Team B (Decom)",
      areaId: null,
      unitId: null,
    },
    {
      ou: "SFD Leeds (Decom)",
      areaId: null,
      unitId: null,
    },
    {
      ou: "MHRA (Decom)",
      areaId: null,
      unitId: null,
    },
    {
      ou: "London - Admin (Decom)",
      areaId: null,
      unitId: null,
    },
    {
      ou: "SFD Cardiff (Decom)",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Specialist Fraud Division WCU",
      areaId: null,
      unitId: null,
    },
    {
      ou: "SFD Liverpool (Decom)",
      areaId: null,
      unitId: null,
    },
    {
      ou: "North Staffs WCU",
      areaId: "13",
      unitId: null,
    },
    {
      ou: "Public Protection Unit (Staffs)",
      areaId: "13",
      unitId: "3",
    },
    {
      ou: "Staffordshire Crown Court Unit",
      areaId: "13",
      unitId: "2",
    },
    {
      ou: "Staffordshire WCU",
      areaId: "13",
      unitId: null,
    },
    {
      ou: "Staffordshire Mags Court Dept",
      areaId: "13",
      unitId: "1",
    },
    {
      ou: "Stafford Default WCU",
      areaId: "13",
      unitId: null,
    },
    {
      ou: "Lichfield WCU",
      areaId: "13",
      unitId: null,
    },
    {
      ou: "Suffolk Default WCU",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Suffolk West WCU (MC) (decom)",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Suffolk Trials WCU (CC)",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Suffolk Trials WCU (MC)",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Suffolk WCU (MC)",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Suffolk Crown Court",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Suffolk Magistrates Court",
      areaId: null,
      unitId: null,
    },
    {
      ou: "Guildford Crown",
      areaId: "9",
      unitId: "2",
    },
    {
      ou: "Do not use - Surrey Default WCU",
      areaId: "9",
      unitId: null,
    },
    {
      ou: "Surrey RASSO",
      areaId: "9",
      unitId: "3",
    },
    {
      ou: "Surrey Witness Care Unit",
      areaId: "9",
      unitId: null,
    },
    {
      ou: "Guildford Mags",
      areaId: "9",
      unitId: "1",
    },
    {
      ou: "Hastings CJU WCU",
      areaId: "9",
      unitId: null,
    },
    {
      ou: "Brighton CJU-WCU",
      areaId: "9",
      unitId: null,
    },
    {
      ou: "TU BRI Satellite WCU",
      areaId: "9",
      unitId: null,
    },
    {
      ou: "Crawley WCU",
      areaId: "9",
      unitId: null,
    },
    {
      ou: "Sussex CJU",
      areaId: "9",
      unitId: null,
    },
    {
      ou: "Sussex RASSO",
      areaId: "9",
      unitId: "3",
    },
    {
      ou: "Sussex TU",
      areaId: "9",
      unitId: "2",
    },
    {
      ou: "Eastbourne CJU WCU",
      areaId: "9",
      unitId: null,
    },
    {
      ou: "CTSU WCU",
      areaId: "9",
      unitId: null,
    },
    {
      ou: "Sussex Default WCU",
      areaId: "9",
      unitId: null,
    },
    {
      ou: "Worthing CJU WCU",
      areaId: "9",
      unitId: null,
    },
    {
      ou: "Brighton TU-WCU",
      areaId: "9",
      unitId: null,
    },
    {
      ou: "Chichester CJU WCU",
      areaId: "9",
      unitId: null,
    },
    {
      ou: "Thames Valley MC",
      areaId: "11",
      unitId: "1",
    },
    {
      ou: "Thames Valley CC",
      areaId: "11",
      unitId: "2",
    },
    {
      ou: "Do Not Use Berkshire WCU",
      areaId: "11",
      unitId: null,
    },
    {
      ou: "Do Not Use Buckinghamshire WCU",
      areaId: "11",
      unitId: null,
    },
    {
      ou: "Thames Chiltern RASSO",
      areaId: "11",
      unitId: "3",
    },
    {
      ou: "Thames Chiltern CCU",
      areaId: "11",
      unitId: "4",
    },
    {
      ou: "WCU Thames Valley",
      areaId: "11",
      unitId: null,
    },
    {
      ou: "Do Not Use Oxfordshire WCU",
      areaId: "11",
      unitId: null,
    },
    {
      ou: "Thames Valley Default WCU",
      areaId: "11",
      unitId: null,
    },
    {
      ou: "Public Protection Unit (Warks)",
      areaId: "13",
      unitId: "3",
    },
    {
      ou: "Warwickshire Magistrates Court Unit",
      areaId: "13",
      unitId: "1",
    },
    {
      ou: "Warwickshire Default WCU",
      areaId: "13",
      unitId: null,
    },
    {
      ou: "Warwickshire Crown Court Unit",
      areaId: "13",
      unitId: "2",
    },
    {
      ou: "Victim and Witness Information Partnership",
      areaId: "13",
      unitId: null,
    },
    {
      ou: "West Mercia Mags Unit",
      areaId: "13",
      unitId: "1",
    },
    {
      ou: "Hereford CU",
      areaId: "13",
      unitId: null,
    },
    {
      ou: "Worcester CU",
      areaId: "13",
      unitId: null,
    },
    {
      ou: "Droitwich TU (Decom)",
      areaId: "13",
      unitId: null,
    },
    {
      ou: "Public Protection Unit (West Mercia)",
      areaId: "13",
      unitId: "3",
    },
    {
      ou: "West Mercia Default WCU",
      areaId: "13",
      unitId: null,
    },
    {
      ou: "West Mercia Crown Court Unit",
      areaId: "13",
      unitId: "2",
    },
    {
      ou: "West Mercia WCU",
      areaId: "13",
      unitId: null,
    },
    {
      ou: "Shropshire CU (Decom)",
      areaId: "13",
      unitId: null,
    },
    {
      ou: "Droitwich CJU (Decom)",
      areaId: "13",
      unitId: null,
    },
    {
      ou: "Brierley Hill WCU",
      areaId: "13",
      unitId: null,
    },
    {
      ou: "Coventry Glidewell WCU",
      areaId: "13",
      unitId: null,
    },
    {
      ou: "West Midlands Crown Court Unit",
      areaId: "13",
      unitId: "2",
    },
    {
      ou: "West Midlands BTP Magistrates Unit",
      areaId: "13",
      unitId: "1",
    },
    {
      ou: "West Midlands BTP Crown Court Unit",
      areaId: "13",
      unitId: "2",
    },
    {
      ou: "Warley WCU",
      areaId: "13",
      unitId: null,
    },
    {
      ou: "Public Protection Unit (Warks)-(not in use)",
      areaId: "13",
      unitId: null,
    },
    {
      ou: "Public Protection Unit (West Mercia)-(not in use)",
      areaId: "13",
      unitId: null,
    },
    {
      ou: "Public Protection Unit (Central)",
      areaId: "13",
      unitId: "3",
    },
    {
      ou: "Public Protection Unit (Central) WCU",
      areaId: "13",
      unitId: null,
    },
    {
      ou: "Western WCU",
      areaId: "13",
      unitId: null,
    },
    {
      ou: "Eastern WCU",
      areaId: "13",
      unitId: null,
    },
    {
      ou: "Central WCU",
      areaId: "13",
      unitId: null,
    },
    {
      ou: "West Bromwich WCU",
      areaId: "13",
      unitId: null,
    },
    {
      ou: "Solihull WCU",
      areaId: "13",
      unitId: null,
    },
    {
      ou: "Walsall WCU",
      areaId: "13",
      unitId: null,
    },
    {
      ou: "Bournville Lane WCU",
      areaId: "13",
      unitId: null,
    },
    {
      ou: "Complex Casework Unit - Midlands",
      areaId: "13",
      unitId: "4",
    },
    {
      ou: "West Midlands Default WCU",
      areaId: "13",
      unitId: null,
    },
    {
      ou: "BTP Birmingham WCU",
      areaId: "13",
      unitId: null,
    },
    {
      ou: "Birmingham Outer WCU",
      areaId: "13",
      unitId: null,
    },
    {
      ou: "Halesowen WCU",
      areaId: "13",
      unitId: null,
    },
    {
      ou: "Birmingham Central WCU",
      areaId: "13",
      unitId: null,
    },
    {
      ou: "Wolverhampton",
      areaId: "13",
      unitId: null,
    },
    {
      ou: "Coventry Criminal Justice Unit (Decom)",
      areaId: "13",
      unitId: null,
    },
    {
      ou: "West Mids Mags Court Dept",
      areaId: "13",
      unitId: "1",
    },
    {
      ou: "Domestic Violence WCU",
      areaId: "13",
      unitId: null,
    },
    {
      ou: "Serious Violence and Organised Crime Unit",
      areaId: "13",
      unitId: "4",
    },
    {
      ou: "Birmingham Trials Unit(Not in use)",
      areaId: "13",
      unitId: null,
    },
    {
      ou: "Weetwood and Pudsey Combined Unit",
      areaId: "14",
      unitId: null,
    },
    {
      ou: "Dewsbury Combined Unit",
      areaId: "14",
      unitId: null,
    },
    {
      ou: "Calder Combined Unit",
      areaId: "14",
      unitId: null,
    },
    {
      ou: "Huddersfield Combined Unit",
      areaId: "14",
      unitId: null,
    },
    {
      ou: "Wakefield Combined Unit",
      areaId: "14",
      unitId: null,
    },
    {
      ou: "Bradford South Combined Unit",
      areaId: "14",
      unitId: null,
    },
    {
      ou: "West Yorkshire Default WCU",
      areaId: "14",
      unitId: null,
    },
    {
      ou: "Pontefract Combined Unit",
      areaId: "14",
      unitId: null,
    },
    {
      ou: "Yorkshire and Humberside Complex Case Unit",
      areaId: "14",
      unitId: "4",
    },
    {
      ou: "West Yorkshire Witness Care Unit",
      areaId: "14",
      unitId: null,
    },
    {
      ou: "West Yorks CPSD",
      areaId: "14",
      unitId: null,
    },
    {
      ou: "North East Leeds Combined Unit (DECOM)",
      areaId: "14",
      unitId: null,
    },
    {
      ou: "Yorkshire and Humberside RASSO Unit",
      areaId: "14",
      unitId: "3",
    },
    {
      ou: "West Yorkshire Mags Court Unit",
      areaId: "14",
      unitId: "1",
    },
    {
      ou: "West Yorkshire Crown Court Unit",
      areaId: "14",
      unitId: "2",
    },
    {
      ou: "Melksham Combined Team (Decom)",
      areaId: "12",
      unitId: null,
    },
    {
      ou: "Swindon Magistrates Unit (decommissioned)",
      areaId: "12",
      unitId: null,
    },
    {
      ou: "Salisbury Combined Team (Decom)",
      areaId: "12",
      unitId: null,
    },
    {
      ou: "Wiltshire Magistrates Unit",
      areaId: "12",
      unitId: "1",
    },
    {
      ou: "Wiltshire Witness Care Unit",
      areaId: "12",
      unitId: null,
    },
    {
      ou: "Wiltshire Default WCU",
      areaId: "12",
      unitId: null,
    },
    {
      ou: "Bedfordshire CC",
      areaId: "11",
      unitId: "2",
    },
    {
      ou: "Lancashire/Cumbria CCU",
      areaId: "8",
      unitId: "4",
    },
  ],
  documentTypes: [
    {
      cmsDocTypeId: "1201",
      docTypeId: "37",
    },
    {
      cmsDocTypeId: "1",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "117",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "118",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "119",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "120",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "121",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "122",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "123",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "124",
      docTypeId: "23",
    },
    {
      cmsDocTypeId: "125",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "126",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "127",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "128",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "129",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "130",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "131",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "132",
      docTypeId: "23",
    },
    {
      cmsDocTypeId: "133",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "134",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "161",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "152",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "153",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "195",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "154",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "155",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "156",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "157",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "227",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "184",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "185",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "2",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "37",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "3",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "4",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "5",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "6",
      docTypeId: "37",
    },
    {
      cmsDocTypeId: "7",
      docTypeId: "38",
    },
    {
      cmsDocTypeId: "8",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "83",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "84",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "85",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "10",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "11",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "99",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "20",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "12",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "191",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "162",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "201",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "13",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "188",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "14",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "15",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "16",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "17",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "18",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "19",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "21",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "22",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "23",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "511",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "24",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "25",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "26",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "89",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "91",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "97",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "98",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "32",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "33",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "34",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "136",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "35",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "36",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "38",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "39",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "40",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "41",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "42",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "43",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "44",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "45",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "46",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "47",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "105",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "111",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "112",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "113",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "114",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "115",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "174",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "48",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "49",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "100252",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "100253",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "178",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "204",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "205",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "50",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "145",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "51",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "146",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "52",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "147",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "53",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "148",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "54",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "149",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "55",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "150",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "56",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "151",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "220",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "138",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "221",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "139",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "140",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "141",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "142",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "143",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "144",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "100232",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "226015",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "186",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "100",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "57",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "58",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "59",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "109",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "108",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "60",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "1064",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "231",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "232",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "229",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "230",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "1065",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "100230",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "206",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "211",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "207",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "208",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "209",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "210",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "197",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "198",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "222",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "223",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "61",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "62",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "63",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "64",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "202",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "65",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "1055",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "137",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "66",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "116",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "216",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "215",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "213",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "214",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "217",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "67",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "100239",
      docTypeId: "39",
    },
    {
      cmsDocTypeId: "226148",
      docTypeId: "39",
    },
    {
      cmsDocTypeId: "100241",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "100240",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "163",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "164",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "228",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "225",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "224",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "1032",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "190",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "200",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "100247",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "100248",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "100245",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "199",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "1066",
      docTypeId: "37",
    },
    {
      cmsDocTypeId: "1001",
      docTypeId: "2",
    },
    {
      cmsDocTypeId: "1015",
      docTypeId: "22",
    },
    {
      cmsDocTypeId: "1031",
      docTypeId: "23",
    },
    {
      cmsDocTypeId: "1016",
      docTypeId: "23",
    },
    {
      cmsDocTypeId: "1059",
      docTypeId: "38",
    },
    {
      cmsDocTypeId: "1017",
      docTypeId: "23",
    },
    {
      cmsDocTypeId: "1018",
      docTypeId: "23",
    },
    {
      cmsDocTypeId: "1019",
      docTypeId: "24",
    },
    {
      cmsDocTypeId: "1040",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "1041",
      docTypeId: "25",
    },
    {
      cmsDocTypeId: "1062",
      docTypeId: "37",
    },
    {
      cmsDocTypeId: "1021",
      docTypeId: "37",
    },
    {
      cmsDocTypeId: "1020",
      docTypeId: "37",
    },
    {
      cmsDocTypeId: "1044",
      docTypeId: "37",
    },
    {
      cmsDocTypeId: "1042",
      docTypeId: "37",
    },
    {
      cmsDocTypeId: "1022",
      docTypeId: "37",
    },
    {
      cmsDocTypeId: "1023",
      docTypeId: "37",
    },
    {
      cmsDocTypeId: "1063",
      docTypeId: "27",
    },
    {
      cmsDocTypeId: "1045",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "1046",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "1047",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "1024",
      docTypeId: "28",
    },
    {
      cmsDocTypeId: "1033",
      docTypeId: "3",
    },
    {
      cmsDocTypeId: "1026",
      docTypeId: "30",
    },
    {
      cmsDocTypeId: "1048",
      docTypeId: "31",
    },
    {
      cmsDocTypeId: "1049",
      docTypeId: "32",
    },
    {
      cmsDocTypeId: "1034",
      docTypeId: "4",
    },
    {
      cmsDocTypeId: "1035",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "1002",
      docTypeId: "5",
    },
    {
      cmsDocTypeId: "1037",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "1003",
      docTypeId: "6",
    },
    {
      cmsDocTypeId: "1004",
      docTypeId: "7",
    },
    {
      cmsDocTypeId: "1005",
      docTypeId: "8",
    },
    {
      cmsDocTypeId: "1060",
      docTypeId: "9",
    },
    {
      cmsDocTypeId: "1061",
      docTypeId: "10",
    },
    {
      cmsDocTypeId: "1036",
      docTypeId: "11",
    },
    {
      cmsDocTypeId: "1006",
      docTypeId: "12",
    },
    {
      cmsDocTypeId: "1038",
      docTypeId: "12",
    },
    {
      cmsDocTypeId: "1039",
      docTypeId: "14",
    },
    {
      cmsDocTypeId: "1008",
      docTypeId: "15",
    },
    {
      cmsDocTypeId: "1009",
      docTypeId: "16",
    },
    {
      cmsDocTypeId: "1010",
      docTypeId: "17",
    },
    {
      cmsDocTypeId: "1011",
      docTypeId: "18",
    },
    {
      cmsDocTypeId: "1012",
      docTypeId: "19",
    },
    {
      cmsDocTypeId: "1013",
      docTypeId: "20",
    },
    {
      cmsDocTypeId: "1014",
      docTypeId: "21",
    },
    {
      cmsDocTypeId: "1027",
      docTypeId: "37",
    },
    {
      cmsDocTypeId: "1028",
      docTypeId: "37",
    },
    {
      cmsDocTypeId: "516",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "1025",
      docTypeId: "29",
    },
    {
      cmsDocTypeId: "1203",
      docTypeId: "37",
    },
    {
      cmsDocTypeId: "101",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "102",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "104",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "103",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "1007",
      docTypeId: "13",
    },
    {
      cmsDocTypeId: "1050",
      docTypeId: "37",
    },
    {
      cmsDocTypeId: "69",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "70",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "71",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "72",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "73",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "74",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "175",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "176",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "177",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "158",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "159",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "196",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "517",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "75",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "76",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "77",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "1030",
      docTypeId: "37",
    },
    {
      cmsDocTypeId: "1200",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "1056",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "1057",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "1058",
      docTypeId: "38",
    },
    {
      cmsDocTypeId: "218",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "219",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "1051",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "1052",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "1053",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "1054",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "100235",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "225583",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "100234",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "225584",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "100236",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "225581",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "100233",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "225582",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "100242",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "100251",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "100244",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "68",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "110",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "160",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "203",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "100243",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "78",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "183",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "189",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "212",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "79",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "181",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "182",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "512",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "513",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "192",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "193",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "194",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "80",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "81",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "1202",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "100249",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "100237",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "100238",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "100231",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "100250",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "100246",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "1029",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "180",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "165",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "166",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "167",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "168",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "169",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "170",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "171",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "172",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "173",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "514",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "515",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "86",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "87",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "88",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "106",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "107",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "187",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "82",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "135",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "179",
      docTypeId: "35",
    },
    {
      cmsDocTypeId: "226",
      docTypeId: "35",
    },
  ],
  investigatingAgencies: [
    {
      ouCode: "00AH",
      investigatingAgencyId: "10",
    },
    {
      ouCode: "00AH",
      investigatingAgencyId: "10",
    },
    {
      ouCode: "00FC",
      investigatingAgencyId: "11",
    },
  ],
};

const dataSource: RedactionLogDataSource = {
  lookUpsData: redactionLogLookUpsData,
  mappingData: redactionLogMappingData,
};
export default dataSource;
