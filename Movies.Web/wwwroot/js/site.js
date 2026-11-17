// Movies.Web client application JavaScript.
// Consumes the Movies.API endpoints with the fetch API (async / await) and renders the
// results with Bootstrap 5 components. No framework or build step is used.
// Security note: data coming from the API is always written to the page with textContent
// or DOM APIs (never with innerHTML) to prevent XSS (Cross-Site Scripting) attacks.
"use strict";

// ------------------------------------------------------------------------------------
// API Base URL
// ------------------------------------------------------------------------------------
// Candidate base URLs of the Movies.API project (see Movies.API/Properties/launchSettings.json).
// When the page loads, each candidate is tried in order with a GET api/Genres request and the
// first one that answers is used for all the requests:
// 1. "http" launch profile (the first and default profile of Movies.API).
// 2. "https" launch profile (the browser must trust the ASP.NET Core development certificate,
//    which can be done by running "dotnet dev-certs https --trust" once).
// In the final phase of the project, the requests will be sent to the Ocelot API Gateway
// instead of directly to Movies.API, therefore only this list will need to be changed to the
// Gateway's URL (for example "http://localhost:5000/api" according to the Gateway's routes).
const API_BASE_URL_CANDIDATES = [
    "http://localhost:5230/api",  // Movies.API "http" profile
    "https://localhost:7230/api"  // Movies.API "https" profile
];

// Maximum time in milliseconds to wait for a candidate API base URL to answer.
const API_CONNECTION_TIMEOUT = 3000;

// The API base URL in use, assigned after a successful connection.
let apiBaseUrl = null;

// Directors and genres lists kept in memory to fill the select lists and the genre
// checkboxes of the movie form and the movie filter without extra requests.
let directors = [];
let genres = [];

// Bootstrap modal instances, created when the page is loaded.
let movieModal = null;
let directorModal = null;
let genreModal = null;

// ------------------------------------------------------------------------------------
// DOM Helpers
// ------------------------------------------------------------------------------------

/**
 * Returns the HTML element with the given id.
 * @param {string} id Element id.
 * @returns {HTMLElement}
 */
function byId(id) {
    return document.getElementById(id);
}

/**
 * Creates an HTML element with optional CSS class names and text content.
 * Text is assigned with textContent so that it is never interpreted as HTML.
 * @param {string} tagName Tag name such as "td" or "button".
 * @param {string} [className] CSS class names.
 * @param {string} [text] Text content.
 * @returns {HTMLElement}
 */
function createElement(tagName, className, text) {
    const element = document.createElement(tagName);
    if (className)
        element.className = className;
    if (text !== undefined && text !== null)
        element.textContent = text;
    return element;
}

/**
 * Creates a table row with a single cell spanning all the columns, used for
 * "Loading...", "No records found." or error rows.
 * @param {number} columnCount Number of the columns of the table.
 * @param {string} text Text of the row.
 * @returns {HTMLTableRowElement}
 */
function createMessageRow(columnCount, text) {
    const row = createElement("tr");
    const cell = createElement("td", "text-center text-muted py-4", text);
    cell.colSpan = columnCount;
    row.append(cell);
    return row;
}

/**
 * Creates the table cell that contains the Edit and Delete buttons of a row.
 * @param {Function} onEdit Function called when the Edit button is clicked.
 * @param {Function} onDelete Function called when the Delete button is clicked.
 * @returns {HTMLTableCellElement}
 */
function createActionsCell(onEdit, onDelete) {
    const cell = createElement("td", "text-end actions-cell");
    const editButton = createElement("button", "btn btn-sm btn-outline-primary me-1", "Edit");
    editButton.type = "button";
    editButton.addEventListener("click", onEdit);
    const deleteButton = createElement("button", "btn btn-sm btn-outline-danger", "Delete");
    deleteButton.type = "button";
    deleteButton.addEventListener("click", onDelete);
    cell.append(editButton, deleteButton);
    return cell;
}

/**
 * Creates the "Movies" cell of the directors and genres tables:
 * the count of the movies and the movie names under it.
 * @param {number} movieCount Count of the movies (MovieCountR).
 * @param {string} movieNames Comma separated movie names (MovieNamesR).
 * @returns {HTMLTableCellElement}
 */
function createMoviesCell(movieCount, movieNames) {
    const cell = createElement("td");
    cell.append(createElement("span", "badge text-bg-secondary", String(movieCount ?? 0)));
    if (movieNames)
        cell.append(createElement("span", "movie-names", movieNames));
    return cell;
}

/**
 * Fills a select list with options.
 * @param {HTMLSelectElement} select Select list to fill.
 * @param {Array} items Items having an id property.
 * @param {Function} getText Function returning the option text of an item.
 * @param {string} emptyText Text of the first option with an empty value.
 * @param {string} [selectedValue] Value to select after filling, if it still exists.
 */
function fillSelect(select, items, getText, emptyText, selectedValue) {
    select.replaceChildren(new Option(emptyText, ""));
    for (const item of items)
        select.append(new Option(getText(item), String(item.id)));
    select.value = selectedValue ?? "";
    if (select.value !== (selectedValue ?? "")) // the selected item no longer exists
        select.value = "";
}

/**
 * Converts a date value returned by the API (e.g. "2010-07-16T00:00:00") to the
 * "yyyy-MM-dd" format required by the value of the date inputs.
 * @param {string} value Date value from the API.
 * @returns {string}
 */
function toDateInputValue(value) {
    return value ? String(value).substring(0, 10) : "";
}

/**
 * Enables or disables the submit button of a form while a request is being sent
 * so that the same request is not sent twice.
 * @param {HTMLFormElement} form Form.
 * @param {boolean} isBusy True to disable, false to enable.
 */
function setFormBusy(form, isBusy) {
    const submitButton = form.querySelector("button[type='submit']");
    if (submitButton)
        submitButton.disabled = isBusy;
}

/**
 * Clears the previous validation state and messages of a form.
 * @param {HTMLFormElement} form Form.
 * @param {HTMLElement} errorsContainer Element that shows the API error messages of the form.
 */
function resetFormState(form, errorsContainer) {
    form.reset();
    form.classList.remove("was-validated");
    errorsContainer.replaceChildren();
}

// ------------------------------------------------------------------------------------
// Alerts
// ------------------------------------------------------------------------------------

// Maximum number of alerts shown at the top of the page at the same time.
const MAX_ALERT_COUNT = 3;

/**
 * Shows one or more messages in a dismissible Bootstrap alert.
 * @param {string|string[]} messages Message or messages.
 * @param {string} type Bootstrap contextual type: "success", "danger", "warning" or "info".
 * @param {HTMLElement} [container] Element to add the alert to, the alerts area at the top
 * of the page by default.
 * @returns {HTMLElement} The created alert element.
 */
function showAlert(messages, type, container) {
    container = container ?? byId("alerts");
    const messageList = Array.isArray(messages) ? messages : [messages];
    const alert = createElement("div", `alert alert-${type} alert-dismissible fade show`);
    alert.setAttribute("role", "alert");
    if (messageList.length === 1) {
        alert.append(createElement("span", null, messageList[0]));
    } else {
        const list = createElement("ul", "mb-0 ps-3");
        for (const message of messageList)
            list.append(createElement("li", null, message));
        alert.append(list);
    }
    const closeButton = createElement("button", "btn-close");
    closeButton.type = "button";
    closeButton.setAttribute("data-bs-dismiss", "alert");
    closeButton.setAttribute("aria-label", "Close");
    alert.append(closeButton);
    container.prepend(alert);
    // Remove the oldest alerts so that the page doesn't fill up with messages.
    while (container.children.length > MAX_ALERT_COUNT)
        container.lastElementChild.remove();
    return alert;
}

// ------------------------------------------------------------------------------------
// API Requests
// ------------------------------------------------------------------------------------

/**
 * Sends an HTTP request to the Movies API and returns the result.
 * The response body is parsed as JSON if possible, otherwise it is returned as text
 * (e.g. GET api/SeedDb returns a plain text message), 204 No Content gives null data.
 * @param {string} method HTTP method: "GET", "POST", "PUT" or "DELETE".
 * @param {string} path Path after the API base URL, e.g. "/Movies" or "/Movies/5".
 * @param {object} [body] Request object that will be sent as JSON in the request body.
 * @returns {Promise<{ok: boolean, status: number, data: any}>}
 * @throws {Error} If the API is not connected or can't be reached (network error).
 */
async function apiRequest(method, path, body) {
    if (!apiBaseUrl)
        throw new Error("The Movies API is not connected! Please start Movies.API and refresh the page.");
    const options = { method: method, headers: {} };
    if (body !== undefined) {
        options.headers["Content-Type"] = "application/json";
        options.body = JSON.stringify(body);
    }
    let response;
    try {
        response = await fetch(apiBaseUrl + path, options);
    } catch {
        // fetch throws only for network errors such as the API being stopped
        // or a CORS problem, HTTP error status codes don't throw.
        throw new Error(`The Movies API could not be reached at ${apiBaseUrl}! ` +
            "Please make sure that Movies.API is running and refresh the page.");
    }
    const text = await response.text();
    let data = null;
    if (text) {
        try {
            data = JSON.parse(text);
        } catch {
            data = text;
        }
    }
    return { ok: response.ok, status: response.status, data: data };
}

/**
 * Extracts the messages to show from an API result:
 * - 400 Bad Request validation errors returned automatically by ASP.NET Core in the
 *   ProblemDetails format: { title, status, errors: { "Name": ["Name is required!"] } }
 * - CommandResponse returned by the controller actions: { isSuccessful, message, id }
 *   (multiple messages are separated by "|" in the message).
 * - Plain text responses such as the one of GET api/SeedDb.
 * @param {{ok: boolean, status: number, data: any}} result Result of apiRequest.
 * @returns {string[]}
 */
function getResponseMessages(result) {
    const data = result.data;
    if (data && typeof data === "object") {
        if (data.errors && typeof data.errors === "object") {
            const messages = Object.values(data.errors).flat().filter(message => message);
            if (messages.length > 0)
                return messages;
        }
        if (typeof data.message === "string" && data.message) {
            const messages = data.message.split("|").filter(message => message);
            if (messages.length > 0)
                return messages;
        }
        if (typeof data.title === "string" && data.title)
            return [data.title];
    }
    if (typeof data === "string" && data)
        return [data];
    return [result.ok ? "Operation completed successfully." : `The request failed with HTTP status code ${result.status}!`];
}

/**
 * Gets a list from the API. 204 No Content (no records) gives an empty array.
 * @param {string} method "GET" for the list endpoints, "POST" for the filter endpoint.
 * @param {string} path Path after the API base URL.
 * @param {object} [body] Request object for the POST requests.
 * @returns {Promise<Array>}
 * @throws {Error} With the API messages if the request is not successful.
 */
async function getList(method, path, body) {
    const result = await apiRequest(method, path, body);
    if (!result.ok)
        throw new Error(getResponseMessages(result).join(" "));
    return Array.isArray(result.data) ? result.data : [];
}

/**
 * Sends a create (POST), update (PUT) or delete (DELETE) request, or the seed request,
 * and shows the API's CommandResponse message: success messages at the top of the page,
 * error messages in the given container (the form's modal or the top of the page).
 * @param {string} method HTTP method.
 * @param {string} path Path after the API base URL.
 * @param {object} [body] Request object.
 * @param {HTMLElement} [errorsContainer] Where to show the error messages.
 * @returns {Promise<boolean>} True if the operation is successful.
 */
async function sendCommand(method, path, body, errorsContainer) {
    errorsContainer = errorsContainer ?? byId("alerts");
    try {
        const result = await apiRequest(method, path, body);
        const messages = getResponseMessages(result);
        if (result.ok) {
            showAlert(messages, "success");
            return true;
        }
        showAlert(messages, "danger", errorsContainer);
    } catch (error) {
        showAlert(error.message, "danger", errorsContainer);
    }
    return false;
}

// ------------------------------------------------------------------------------------
// API Connection
// ------------------------------------------------------------------------------------

/**
 * Checks whether the Movies API answers at the given base URL.
 * redirect: "manual" is used because when Movies.API runs with the "https" profile,
 * its HTTP address redirects to HTTPS (app.UseHttpsRedirection()) and CORS preflight
 * requests can't follow redirects, so a redirecting address is skipped.
 * @param {string} baseUrl Candidate API base URL.
 * @returns {Promise<boolean>}
 */
async function isApiAvailable(baseUrl) {
    const controller = new AbortController();
    const timeoutId = setTimeout(() => controller.abort(), API_CONNECTION_TIMEOUT);
    try {
        const response = await fetch(baseUrl + "/Genres", { signal: controller.signal, redirect: "manual" });
        return response.ok; // 200 OK or 204 No Content
    } catch {
        return false;
    } finally {
        clearTimeout(timeoutId);
    }
}

/**
 * Updates the API connection indicator in the navigation bar.
 * @param {string} type Bootstrap contextual type of the badge.
 * @param {string} text Text of the badge.
 */
function setApiStatus(type, text) {
    const apiStatus = byId("apiStatus");
    apiStatus.className = `badge text-bg-${type} api-status`;
    apiStatus.textContent = text;
    apiStatus.title = text;
}

/**
 * Shows a warning alert explaining how to start Movies.API with a Retry button.
 */
function showConnectionHelp() {
    const alert = showAlert("The Movies API could not be reached!", "warning");
    const details = createElement("div", "mt-2");
    details.append(createElement("div", null, "Tried: " + API_BASE_URL_CANDIDATES.join(", ")));
    const steps = createElement("ul", "mb-2 ps-3");
    steps.append(
        createElement("li", null, "Visual Studio: right click the solution, select Configure Startup Projects..., " +
            "choose Multiple startup projects and set the Action of Movies.API and Movies.Web to Start, then run."),
        createElement("li", null, "Command line: run \"dotnet run --project Movies.API --launch-profile http\" " +
            "in the solution folder, then click Retry."),
        createElement("li", null, "If Movies.API is started with the https profile, the development certificate must be " +
            "trusted by the browser (dotnet dev-certs https --trust)."));
    details.append(steps);
    const retryButton = createElement("button", "btn btn-sm btn-warning", "Retry");
    retryButton.type = "button";
    retryButton.addEventListener("click", () => {
        alert.remove();
        connectToApi();
    });
    details.append(retryButton);
    // Insert the details before the close button of the alert.
    alert.insertBefore(details, alert.querySelector(".btn-close"));
}

/**
 * Finds the first answering API base URL candidate, then loads all the lists.
 */
async function connectToApi() {
    apiBaseUrl = null;
    setApiStatus("secondary", "API: connecting...");
    for (const candidate of API_BASE_URL_CANDIDATES) {
        if (await isApiAvailable(candidate)) {
            apiBaseUrl = candidate;
            break;
        }
    }
    if (!apiBaseUrl) {
        setApiStatus("danger", "API: not reachable");
        showConnectionHelp();
        return;
    }
    setApiStatus("success", "API: " + apiBaseUrl);
    // Swagger is at the root of the API's origin: e.g. http://localhost:5230/swagger
    byId("swaggerLink").href = new URL(apiBaseUrl).origin + "/swagger";
    await reloadAll();
}

/**
 * Reloads all the lists: directors and genres first since their select lists and
 * checkboxes are used by the movie filter and the movie form, then the movies.
 * Called after every create, update, delete or seed operation since a change may
 * affect the other lists (e.g. movie counts of the directors and genres).
 */
async function reloadAll() {
    await Promise.all([loadDirectors(), loadGenres()]);
    await loadMovies();
}

// ------------------------------------------------------------------------------------
// Movies
// ------------------------------------------------------------------------------------

/**
 * Creates the request object of POST api/Movies/Filter from the filter form.
 * Only the filled values are added, therefore an empty filter returns all the movies.
 * @returns {object} e.g. { name: "dark", directorId: 2, releaseDateStart: "2000-01-01" }
 */
function getFilterRequest() {
    const request = {};
    const name = byId("filterName").value.trim();
    if (name)
        request.name = name;
    if (byId("filterDirectorId").value)
        request.directorId = Number(byId("filterDirectorId").value);
    if (byId("filterGenreId").value)
        request.genreId = Number(byId("filterGenreId").value);
    if (byId("filterReleaseDateStart").value)
        request.releaseDateStart = byId("filterReleaseDateStart").value;
    if (byId("filterReleaseDateEnd").value)
        request.releaseDateEnd = byId("filterReleaseDateEnd").value;
    if (byId("filterTotalRevenueMin").value !== "")
        request.totalRevenueMin = Number(byId("filterTotalRevenueMin").value);
    if (byId("filterTotalRevenueMax").value !== "")
        request.totalRevenueMax = Number(byId("filterTotalRevenueMax").value);
    return request;
}

/**
 * Loads the movies: GET api/Movies if the filter is empty,
 * otherwise POST api/Movies/Filter with the filled filter values.
 */
async function loadMovies() {
    const tableBody = byId("moviesTableBody");
    tableBody.replaceChildren(createMessageRow(6, "Loading..."));
    try {
        const filter = getFilterRequest();
        const movies = Object.keys(filter).length > 0
            ? await getList("POST", "/Movies/Filter", filter)
            : await getList("GET", "/Movies");
        renderMovies(movies);
    } catch (error) {
        tableBody.replaceChildren(createMessageRow(6, "Movies could not be loaded."));
        showAlert(error.message, "danger");
    }
}

/**
 * Renders the movies table using the formatted response properties ending with R.
 * @param {Array} movies Movie query responses.
 */
function renderMovies(movies) {
    const tableBody = byId("moviesTableBody");
    if (movies.length === 0) {
        tableBody.replaceChildren(createMessageRow(6, "No movies found."));
        return;
    }
    tableBody.replaceChildren(...movies.map(movie => {
        const row = createElement("tr");
        row.append(
            createElement("td", "fw-semibold", movie.name),
            createElement("td", null, movie.releaseDateR),
            createElement("td", "text-end", movie.totalRevenueR),
            createElement("td", null, movie.directorFullNameR),
            createElement("td", null, movie.genreNamesR),
            createActionsCell(() => openMovieModal(movie.id), () => deleteMovie(movie)));
        return row;
    }));
}

/**
 * Renders the genre checkboxes of the movie form.
 * @param {number[]} checkedGenreIds Ids of the genres to check.
 */
function renderGenreCheckboxes(checkedGenreIds) {
    const container = byId("movieGenres");
    container.classList.remove("is-invalid");
    if (genres.length === 0) {
        container.replaceChildren(createElement("span", "text-muted", "No genres found. Please add a genre first."));
        return;
    }
    container.replaceChildren(...genres.map(genre => {
        const formCheck = createElement("div", "form-check");
        const checkbox = createElement("input", "form-check-input");
        checkbox.type = "checkbox";
        checkbox.id = "movieGenre" + genre.id;
        checkbox.value = String(genre.id);
        checkbox.checked = checkedGenreIds.includes(genre.id);
        const label = createElement("label", "form-check-label", genre.name);
        label.htmlFor = checkbox.id;
        formCheck.append(checkbox, label);
        return formCheck;
    }));
}

/**
 * Returns the ids of the checked genres in the movie form.
 * @returns {number[]}
 */
function getCheckedGenreIds() {
    return Array.from(byId("movieGenres").querySelectorAll("input[type='checkbox']:checked"))
        .map(checkbox => Number(checkbox.value));
}

/**
 * Opens the movie modal for creating a new movie (no id) or editing an existing one.
 * For editing, the movie is retrieved with GET api/Movies/{id} to fill the form.
 * @param {number} [id] Movie id to edit.
 */
async function openMovieModal(id) {
    const form = byId("movieForm");
    resetFormState(form, byId("movieFormErrors"));
    fillSelect(byId("movieDirectorId"), directors, director => director.fullNameR, "-- Select Director --");
    byId("movieId").value = "";
    let checkedGenreIds = [];
    if (id) {
        let movie = null;
        try {
            const result = await apiRequest("GET", `/Movies/${id}`);
            if (!result.ok) {
                showAlert(getResponseMessages(result), "danger");
                return;
            }
            movie = result.data;
        } catch (error) {
            showAlert(error.message, "danger");
            return;
        }
        if (!movie) { // 204 No Content: the movie may have been deleted
            showAlert("Movie not found!", "warning");
            await loadMovies();
            return;
        }
        byId("movieModalTitle").textContent = "Edit Movie";
        byId("movieId").value = String(movie.id);
        byId("movieName").value = movie.name;
        byId("movieReleaseDate").value = toDateInputValue(movie.releaseDate);
        byId("movieTotalRevenue").value = String(movie.totalRevenue);
        byId("movieDirectorId").value = String(movie.directorId);
        checkedGenreIds = movie.genreIds ?? [];
    } else {
        byId("movieModalTitle").textContent = "New Movie";
    }
    renderGenreCheckboxes(checkedGenreIds);
    movieModal.show();
}

/**
 * Validates the movie form and sends POST api/Movies (create) or PUT api/Movies (update).
 * Request JSON (MovieAddRequest / MovieUpdateRequest):
 * { id, name, releaseDate, totalRevenue, directorId, genreIds }
 * @param {SubmitEvent} event Form submit event.
 */
async function saveMovie(event) {
    event.preventDefault();
    const form = byId("movieForm");
    const genreIds = getCheckedGenreIds();
    byId("movieGenres").classList.toggle("is-invalid", genreIds.length === 0);
    form.classList.add("was-validated");
    if (!form.checkValidity() || genreIds.length === 0)
        return;
    const id = byId("movieId").value;
    const request = {
        name: byId("movieName").value.trim(),
        releaseDate: byId("movieReleaseDate").value || null, // "yyyy-MM-dd" or null
        totalRevenue: Number(byId("movieTotalRevenue").value),
        directorId: Number(byId("movieDirectorId").value),
        genreIds: genreIds
    };
    if (id)
        request.id = Number(id);
    byId("movieFormErrors").replaceChildren();
    setFormBusy(form, true);
    const isSuccessful = await sendCommand(id ? "PUT" : "POST", "/Movies", request, byId("movieFormErrors"));
    setFormBusy(form, false);
    if (isSuccessful) {
        movieModal.hide();
        await reloadAll();
    }
}

/**
 * Deletes a movie with DELETE api/Movies/{id} after confirmation.
 * @param {object} movie Movie query response.
 */
async function deleteMovie(movie) {
    if (!confirm(`Are you sure you want to delete the movie "${movie.name}"?`))
        return;
    if (await sendCommand("DELETE", `/Movies/${movie.id}`))
        await reloadAll();
}

// ------------------------------------------------------------------------------------
// Directors
// ------------------------------------------------------------------------------------

/**
 * Loads the directors with GET api/Directors, renders the directors table and
 * refreshes the director select list of the movie filter.
 */
async function loadDirectors() {
    const tableBody = byId("directorsTableBody");
    tableBody.replaceChildren(createMessageRow(5, "Loading..."));
    try {
        directors = await getList("GET", "/Directors");
        renderDirectors();
    } catch (error) {
        directors = [];
        tableBody.replaceChildren(createMessageRow(5, "Directors could not be loaded."));
        showAlert(error.message, "danger");
    }
    const filterSelect = byId("filterDirectorId");
    fillSelect(filterSelect, directors, director => director.fullNameR, "-- All Directors --", filterSelect.value);
}

/**
 * Renders the directors table.
 */
function renderDirectors() {
    const tableBody = byId("directorsTableBody");
    if (directors.length === 0) {
        tableBody.replaceChildren(createMessageRow(5, "No directors found."));
        return;
    }
    tableBody.replaceChildren(...directors.map(director => {
        const row = createElement("tr");
        row.append(
            createElement("td", null, director.firstName),
            createElement("td", null, director.lastName),
            createElement("td", null, director.isRetiredR),
            createMoviesCell(director.movieCountR, director.movieNamesR),
            createActionsCell(() => openDirectorModal(director.id), () => deleteDirector(director)));
        return row;
    }));
}

/**
 * Opens the director modal for creating a new director (no id) or editing an existing one.
 * For editing, the director is retrieved with GET api/Directors/{id} to fill the form.
 * @param {number} [id] Director id to edit.
 */
async function openDirectorModal(id) {
    const form = byId("directorForm");
    resetFormState(form, byId("directorFormErrors"));
    byId("directorId").value = "";
    if (id) {
        let director = null;
        try {
            const result = await apiRequest("GET", `/Directors/${id}`);
            if (!result.ok) {
                showAlert(getResponseMessages(result), "danger");
                return;
            }
            director = result.data;
        } catch (error) {
            showAlert(error.message, "danger");
            return;
        }
        if (!director) { // 204 No Content
            showAlert("Director not found!", "warning");
            await loadDirectors();
            return;
        }
        byId("directorModalTitle").textContent = "Edit Director";
        byId("directorId").value = String(director.id);
        byId("directorFirstName").value = director.firstName;
        byId("directorLastName").value = director.lastName;
        byId("directorIsRetired").checked = director.isRetired;
    } else {
        byId("directorModalTitle").textContent = "New Director";
    }
    directorModal.show();
}

/**
 * Validates the director form and sends POST api/Directors (create) or PUT api/Directors (update).
 * Request JSON (DirectorAddRequest / DirectorUpdateRequest): { id, firstName, lastName, isRetired }
 * @param {SubmitEvent} event Form submit event.
 */
async function saveDirector(event) {
    event.preventDefault();
    const form = byId("directorForm");
    form.classList.add("was-validated");
    if (!form.checkValidity())
        return;
    const id = byId("directorId").value;
    const request = {
        firstName: byId("directorFirstName").value.trim(),
        lastName: byId("directorLastName").value.trim(),
        isRetired: byId("directorIsRetired").checked
    };
    if (id)
        request.id = Number(id);
    byId("directorFormErrors").replaceChildren();
    setFormBusy(form, true);
    const isSuccessful = await sendCommand(id ? "PUT" : "POST", "/Directors", request, byId("directorFormErrors"));
    setFormBusy(form, false);
    if (isSuccessful) {
        directorModal.hide();
        await reloadAll();
    }
}

/**
 * Deletes a director with DELETE api/Directors/{id} after confirmation.
 * The API doesn't delete a director having movies and returns an error message.
 * @param {object} director Director query response.
 */
async function deleteDirector(director) {
    if (!confirm(`Are you sure you want to delete the director "${director.fullNameR}"?`))
        return;
    if (await sendCommand("DELETE", `/Directors/${director.id}`))
        await reloadAll();
}

// ------------------------------------------------------------------------------------
// Genres
// ------------------------------------------------------------------------------------

/**
 * Loads the genres with GET api/Genres, renders the genres table and
 * refreshes the genre select list of the movie filter.
 */
async function loadGenres() {
    const tableBody = byId("genresTableBody");
    tableBody.replaceChildren(createMessageRow(3, "Loading..."));
    try {
        genres = await getList("GET", "/Genres");
        renderGenres();
    } catch (error) {
        genres = [];
        tableBody.replaceChildren(createMessageRow(3, "Genres could not be loaded."));
        showAlert(error.message, "danger");
    }
    const filterSelect = byId("filterGenreId");
    fillSelect(filterSelect, genres, genre => genre.name, "-- All Genres --", filterSelect.value);
}

/**
 * Renders the genres table.
 */
function renderGenres() {
    const tableBody = byId("genresTableBody");
    if (genres.length === 0) {
        tableBody.replaceChildren(createMessageRow(3, "No genres found."));
        return;
    }
    tableBody.replaceChildren(...genres.map(genre => {
        const row = createElement("tr");
        row.append(
            createElement("td", "fw-semibold", genre.name),
            createMoviesCell(genre.movieCountR, genre.movieNamesR),
            createActionsCell(() => openGenreModal(genre.id), () => deleteGenre(genre)));
        return row;
    }));
}

/**
 * Opens the genre modal for creating a new genre (no id) or editing an existing one.
 * For editing, the genre is retrieved with GET api/Genres/{id} to fill the form.
 * @param {number} [id] Genre id to edit.
 */
async function openGenreModal(id) {
    const form = byId("genreForm");
    resetFormState(form, byId("genreFormErrors"));
    byId("genreId").value = "";
    if (id) {
        let genre = null;
        try {
            const result = await apiRequest("GET", `/Genres/${id}`);
            if (!result.ok) {
                showAlert(getResponseMessages(result), "danger");
                return;
            }
            genre = result.data;
        } catch (error) {
            showAlert(error.message, "danger");
            return;
        }
        if (!genre) { // 204 No Content
            showAlert("Genre not found!", "warning");
            await loadGenres();
            return;
        }
        byId("genreModalTitle").textContent = "Edit Genre";
        byId("genreId").value = String(genre.id);
        byId("genreName").value = genre.name;
    } else {
        byId("genreModalTitle").textContent = "New Genre";
    }
    genreModal.show();
}

/**
 * Validates the genre form and sends POST api/Genres (create) or PUT api/Genres (update).
 * Request JSON (GenreAddRequest / GenreUpdateRequest): { id, name }
 * @param {SubmitEvent} event Form submit event.
 */
async function saveGenre(event) {
    event.preventDefault();
    const form = byId("genreForm");
    form.classList.add("was-validated");
    if (!form.checkValidity())
        return;
    const id = byId("genreId").value;
    const request = {
        name: byId("genreName").value.trim()
    };
    if (id)
        request.id = Number(id);
    byId("genreFormErrors").replaceChildren();
    setFormBusy(form, true);
    const isSuccessful = await sendCommand(id ? "PUT" : "POST", "/Genres", request, byId("genreFormErrors"));
    setFormBusy(form, false);
    if (isSuccessful) {
        genreModal.hide();
        await reloadAll();
    }
}

/**
 * Deletes a genre with DELETE api/Genres/{id} after confirmation.
 * @param {object} genre Genre query response.
 */
async function deleteGenre(genre) {
    if (!confirm(`Are you sure you want to delete the genre "${genre.name}"?`))
        return;
    if (await sendCommand("DELETE", `/Genres/${genre.id}`))
        await reloadAll();
}

// ------------------------------------------------------------------------------------
// Sample Data
// ------------------------------------------------------------------------------------

/**
 * Resets the database to the sample data with GET api/SeedDb after confirmation.
 */
async function resetSampleData() {
    if (!confirm("All the movies, directors and genres will be deleted and the sample data will be created again. Do you want to continue?"))
        return;
    if (await sendCommand("GET", "/SeedDb")) {
        byId("filterForm").reset(); // ids change after seeding, so the old filter values are cleared
        await reloadAll();
    }
}

// ------------------------------------------------------------------------------------
// Page Initialization
// ------------------------------------------------------------------------------------

/**
 * Registers the event handlers and connects to the API when the page is loaded.
 */
function initialize() {
    // Bootstrap's JavaScript is loaded from a CDN, therefore an internet connection is required.
    if (typeof bootstrap === "undefined") {
        showAlert("Bootstrap could not be loaded from the CDN, please check your internet connection " +
            "and refresh the page.", "danger");
        return;
    }
    movieModal = new bootstrap.Modal(byId("movieModal"));
    directorModal = new bootstrap.Modal(byId("directorModal"));
    genreModal = new bootstrap.Modal(byId("genreModal"));

    byId("newMovieButton").addEventListener("click", () => openMovieModal());
    byId("newDirectorButton").addEventListener("click", () => openDirectorModal());
    byId("newGenreButton").addEventListener("click", () => openGenreModal());

    byId("movieForm").addEventListener("submit", saveMovie);
    byId("directorForm").addEventListener("submit", saveDirector);
    byId("genreForm").addEventListener("submit", saveGenre);

    // Updates the genres validation message while the checkboxes are being checked.
    byId("movieGenres").addEventListener("change", () => {
        if (byId("movieForm").classList.contains("was-validated"))
            byId("movieGenres").classList.toggle("is-invalid", getCheckedGenreIds().length === 0);
    });

    byId("filterForm").addEventListener("submit", event => {
        event.preventDefault();
        loadMovies();
    });
    byId("clearFilterButton").addEventListener("click", () => {
        byId("filterForm").reset();
        loadMovies();
    });

    byId("resetButton").addEventListener("click", resetSampleData);

    // On small screens the navigation bar items are shown in a collapsible menu,
    // the menu is closed after a tab (Movies, Directors or Genres) is selected.
    for (const tab of document.querySelectorAll("#mainNavbar [data-bs-toggle='tab']")) {
        tab.addEventListener("click", () => {
            if (byId("mainNavbar").classList.contains("show"))
                bootstrap.Collapse.getOrCreateInstance(byId("mainNavbar")).hide();
        });
    }

    connectToApi();
}

// The script is referenced at the end of the body, so the page elements already exist.
initialize();
