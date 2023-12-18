`CaseDetails.ts` will change to have `witnesses: []` added

```
id: number,
....
witnesses: [
    {
    "id": 2762766,
    "shoulderNumber": string | null,
    "title": string | "Prof",
    "name": string | "Percy Plum",
    "hasStatements": boolean , //"Y"
    "listOrder": 1,

    "child": boolean ,
    "expert": boolean ,
    "greatestNeed": boolean ,
    "prisoner": boolean ,
    "interpreter": boolean ,
    "vulnerable": boolean ,
    "police": boolean ,
    "professional": boolean ,
    "specialNeeds": boolean ,
    "intimidated": boolean ,
    "victim": boolean

    }
]

```

`PresentationDocumentProperties` in `PresentationDocument.ts` will have

```
    witnessId: null | number
```

`mapDocumentsState.ts` should take in the case details (or maybe just the witnesses array)
so it can splice a new `witnessIndicators` array into `MappedCaseDocument.ts`.

```
    witnessIndicators: ["G", "H"]
    witnessIndicators: (keyof WitnessIndicators)[]
```

WitnessIndicators["G"] // gives full test
