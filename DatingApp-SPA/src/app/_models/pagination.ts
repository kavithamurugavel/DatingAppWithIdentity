export interface Pagination {
    // these variables coincide with the header returning from the API
    currentPage: number;
    itemsPerPage: number;
    totalItems: number;
    totalPages: number;
}

// store items for eg: users, as well as the pagination information
export class PaginatedResult<T> {
    result: T; // store the items like users
    pagination: Pagination;
}
