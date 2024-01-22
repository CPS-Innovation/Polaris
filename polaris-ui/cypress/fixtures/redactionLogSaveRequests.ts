export const expectedUnderRedactionType1Payload = {
  urn: "99ZZ9999999",
  unit: {
    id: "9-1",
    type: "Area",
    areaDivisionName: "South East",
    name: "Magistrates Court",
  },
  investigatingAgency: { id: "43", name: "Greater Manchester Police" },
  documentType: { id: "35", name: "Other" },
  redactions: [
    {
      missedRedaction: { id: "4", name: "Relationship to others" },
      redactionType: 1,
      returnedToInvestigativeAuthority: false,
    },
    {
      missedRedaction: { id: "4", name: "Relationship to others" },
      redactionType: 1,
      returnedToInvestigativeAuthority: false,
    },
  ],
  notes: "hello notes",
  chargeStatus: 2,
  cmsValues: {
    originalFileName: "M*******3",
    documentId: "1",
    documentType: "MG11",
    fileCreatedDate: "2020-06-01",
    documentTypeId: 1,
  },
};

export const expectedUnderRedactionType2Payload = {
  urn: "99ZZ9999999",
  unit: {
    id: "9-1",
    type: "Area",
    areaDivisionName: "South East",
    name: "Magistrates Court",
  },
  investigatingAgency: { id: "43", name: "Greater Manchester Police" },
  documentType: { id: "35", name: "Other" },
  redactions: [
    {
      missedRedaction: { id: "1", name: "Named individual" },
      redactionType: 1,
      returnedToInvestigativeAuthority: true,
    },
    {
      missedRedaction: { id: "5", name: "Address" },
      redactionType: 1,
      returnedToInvestigativeAuthority: true,
    },
  ],
  notes: "hello",
  chargeStatus: 2,
  cmsValues: {
    originalFileName: "M*******3",
    documentId: "1",
    documentType: "MG11",
    fileCreatedDate: "2020-06-01",
    documentTypeId: 1,
  },
};

export const expectedOverRedactionLogPayload = {
  urn: "99ZZ9999999",
  unit: {
    id: "9-1",
    type: "Area",
    areaDivisionName: "South East",
    name: "Magistrates Court",
  },
  investigatingAgency: { id: "43", name: "Greater Manchester Police" },
  documentType: { id: "35", name: "Other" },
  redactions: [
    {
      missedRedaction: { id: "2", name: "Title" },
      redactionType: 2,
      returnedToInvestigativeAuthority: true,
    },
    {
      missedRedaction: { id: "6", name: "Location" },
      redactionType: 2,
      returnedToInvestigativeAuthority: true,
    },
  ],
  notes: "hello notes",
  chargeStatus: 2,
  cmsValues: {
    originalFileName: "M*******3",
    documentId: "1",
    documentType: "MG11",
    fileCreatedDate: "2020-06-01",
    documentTypeId: 1,
  },
};
export const expectedOverUnderRedactionLogPayload = {
  urn: "99ZZ9999999",
  unit: {
    id: "9-1",
    type: "Area",
    areaDivisionName: "South East",
    name: "Magistrates Court",
  },
  investigatingAgency: { id: "43", name: "Greater Manchester Police" },
  documentType: { id: "35", name: "Other" },
  redactions: [
    {
      missedRedaction: { id: "6", name: "Location" },
      redactionType: 1,
      returnedToInvestigativeAuthority: true,
    },
    {
      missedRedaction: { id: "8", name: "NHS number" },
      redactionType: 1,
      returnedToInvestigativeAuthority: true,
    },
    {
      missedRedaction: { id: "3", name: "Occupation" },
      redactionType: 2,
      returnedToInvestigativeAuthority: false,
    },
    {
      missedRedaction: { id: "7", name: "Vehicle registration" },
      redactionType: 2,
      returnedToInvestigativeAuthority: false,
    },
  ],
  notes: null,
  chargeStatus: 2,
  cmsValues: {
    originalFileName: "M*******3",
    documentId: "1",
    documentType: "MG11",
    fileCreatedDate: "2020-06-01",
    documentTypeId: 1,
  },
};
