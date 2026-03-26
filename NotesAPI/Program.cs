var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(new NoteRepository("Data Source=notes.db"));

var app = builder.Build();

app.UseHttpsRedirection();

// GET all notes
app.MapGet("/notes", (NoteRepository repo) =>
    Results.Ok(repo.GetAll()));

// GET single note
app.MapGet("/notes/{id}", (int id, NoteRepository repo) =>
{
    var note = repo.GetById(id);
    return note is null ? Results.NotFound() : Results.Ok(note);
});

// POST create note
app.MapPost("/notes", (NoteRepository repo) =>
{
    var note = repo.Create();
    return Results.Created($"/notes/{note.Id}", note);
});

// PUT update note
app.MapPut("/notes/{id}", (int id, Note updated, NoteRepository repo) =>
{
    var success = repo.Update(id, updated);
    return success ? Results.Ok() : Results.NotFound();
});

// DELETE note
app.MapDelete("/notes/{id}", (int id, NoteRepository repo) =>
{
    var success = repo.Delete(id);
    return success ? Results.NoContent() : Results.NotFound();
});

app.Run();
