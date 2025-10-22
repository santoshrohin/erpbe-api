# API Filtering, Searching & Sorting Examples

## Farmer API Examples

### 1. Get All Farmers (Simple)
```
GET /api/Farmer
```

### 2. Get Farmers with Pagination
```
GET /api/Farmer?PageNumber=1&PageSize=10
```

### 3. Search Farmers by Name
```
GET /api/Farmer?SearchTerm=John
```

### 4. Filter Farmers by Branch
```
GET /api/Farmer?BranchId=1
```

### 5. Sort Farmers by Name (Descending)
```
GET /api/Farmer?SortBy=FarmerName&SortDirection=desc
```

### 6. Complex Query - Search, Filter, Sort, and Paginate
```
GET /api/Farmer?SearchTerm=farm&BranchId=1&SortBy=FarmerName&SortDirection=asc&PageNumber=1&PageSize=5
```

### 7. Filter by Multiple Criteria
```
GET /api/Farmer?FarmerName=John&FarmerCode=F001&BranchId=1&LineId=2
```

## Branch API Examples

### 1. Get All Branches
```
GET /api/Branch
```

### 2. Search Branches
```
GET /api/Branch?SearchTerm=North
```

### 3. Paginated Branches
```
GET /api/Branch?PageNumber=1&PageSize=5&SortBy=BranchName&SortDirection=asc
```

## Response Format

### Paged Response
```json
{
  "data": [
    {
      "farmerId": 1,
      "farmerName": "John Doe",
      "farmerCode": "F001",
      "farmerAddress": "123 Farm St",
      "branchId": 1,
      "lineId": 2
    }
  ],
  "totalCount": 100,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 10,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

### Simple List Response (Backward Compatibility)
```json
[
  {
    "farmerId": 1,
    "farmerName": "John Doe",
    "farmerCode": "F001",
    "farmerAddress": "123 Farm St",
    "branchId": 1,
    "lineId": 2
  }
]
```

## Query Parameters

### Common Parameters
- `PageNumber` (int): Page number (default: 1)
- `PageSize` (int): Items per page (default: 10, max: 100)
- `SortBy` (string): Field to sort by
- `SortDirection` (string): "asc" or "desc" (default: "asc")
- `SearchTerm` (string): Global search term

### Farmer-Specific Parameters
- `BranchId` (int): Filter by branch ID
- `LineId` (int): Filter by line ID
- `FarmerCode` (string): Filter by farmer code
- `FarmerName` (string): Filter by farmer name

### Branch-Specific Parameters
- `BranchName` (string): Filter by branch name

## Error Responses

### Validation Error
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "PageSize": ["Page size must be between 1 and 100"],
    "SortBy": ["Invalid sort field. Valid fields are: FarmerId, FarmerName, FarmerCode, FarmerAddress, BranchId, LineId"]
  }
}
```

## Performance Notes

1. **Pagination**: Always use pagination for large datasets
2. **Search**: Search is case-insensitive and uses LIKE operator
3. **Sorting**: Only valid fields can be used for sorting
4. **Filters**: Multiple filters are combined with AND logic
5. **Indexing**: Ensure proper database indexes on frequently searched/sorted columns
