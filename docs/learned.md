# What I learned

## B01 · unit tests + CI (30 Oct 2026)
- `ci.yml` is the GitHub Actions workflow. On a push or PR to `main`, it checks out the repo, installs .NET, restores the NuGet packages, builds, and runs the tests. If any step fails, the build goes red.
- Unit tests only test behaviour: whether the methods do what they're supposed to. The dependencies are faked.
- Tests can be written before the code, so the code is developed to pass them.
- Fakes are assumptions, not the real thing. They can be wrong, because an assumption may depend on other behaviour that gets missed.
- The sender came from the request body, so a logged-in user could impersonate someone else. Now it comes from the JWT.
- Even then, anyone with a thread ID could post into it. There was no membership check. Now non-members get 403.
