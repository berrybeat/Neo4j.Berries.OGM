# How to contribute

We welcome community pull requests for enhancements and bug fixes. This document is a guide for any member of the community who wish to add/fix something in this repository. Here we work together, move together and achieve together.

## Type of issues

In this repository, any pull request must be related to an open issue. In this repository there are two type of issues in general. Which are explained below.

### Enhancements
These type of issues are when a new feature is going to be added or an existing one will be improved, also maybe an existing feature gets deprecated because a better one is offered and developed. These issues will be labeled as `enhancement`. In order to offer a new feature please follow these steps:
1. Please make sure that a related issues doesn't already exist. If it does and you have extra ideas for it, please offer them under the found issue.
    * if the issue is not picked yet, you can discuss the new ideas and pick the issue yourself
    * if the issue i already picked, you should discuss with the developer who has picked the issue to agree on some terms on how to implement the new ideas when approved
2. To open a new `enhancement`, please make sure you are using the right template and be descriptive as possible regarding your idea and the enhancement you want to contribute to this repository.
3. If the design of the `enhancement` is clear, the feature could be immediately picked by you or anyone else and be started, but if there is no agreement on top of the design yet, it should wait until there is a conclusion from the berrybeat's team and the community regarding the feature design.
4. If all above are good to go, you need to wait until the feature is addressed and can be started, but of course you could start it immediately and the result of implementation will be discussed by the reviewer.

### Bug fixes
These type of issues are when an expected feature from the repository is misbehaving. These type of issues are usually small, but small doesn't mean easy, since the root of the bug must be found and tackled. The bugs are labeled as `incident`. In case you have encountered an `incident` please take the following steps to report:

1. Please first make sure the `incident` is not already reported. It saves a lot of time for us when checking the open issues.
2. In case you have decided now to open a new issue, make sure you have selected the correct template.
3. Please make sure to be as descriptive as possible while describing the issue. This makes the conversation and the investigation much easier and faster :)


## Development flow
Regardless of the issue type, if you decided to contribute as a developer and fix/enhance something, it would not hurt to have a look here:

* Setup your development environment. You will need to run a local neo4j database to run the unit tests.
* Make sure the work you are trying to do on an issue is already communicated with the team. **This communication is very important to prevent throwing lots of work done.**
* [Create a fork](https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/working-with-forks/fork-a-repo), then start implementing the issue under a new local branch. The branch will be used to create the pull request later.
* We use Test-Driven-Development in this repository. Make sure on adding new features you are following the same test pattern as much as possible.
* On adding new code to the repository, make sure you are following the existing patterns you see in the existing code.
* When your changes are done, please make sure all unit tests are still running. The `PR` won't be approved if the tests are not being passed.
* Please always consider situations, where your changes are causing other areas of the repository need more unit tests.
* Commit changes and push the local branch to your Github fork.
* Create a PR for the issue you were working on.
* Wait for someone to pick your changes and review them.
* Discuss with the reviewer and be transparent regarding your changes.
* Congratulations, when all above are done, you have made your contribution successfully to this repository.

## Breaking changes
As long as the repository is not meant to release a breaking change, make sure you have backward/forward compatibility in mind while implementing issues. The older methods which won't be supported in the next breaking version, must be marked as deprecated with a hint on how to migrate to the newer method.


## Labels

* **enhancement:** As described before, an `enhancement` is when a new feature is being added or an existing one is being improved.
* **incident:** As described before, an `incident` is simply equivalent to a bug report.
* **needs-design:** This is when an issue still needs some considerations and agreements on the design, how to implement.
* **easy-peasy:** These are the type of issues, which are pretty straight forward to do.
* **hardcore:** There are the issues which the design the clear, but implementing the issue is complex.

## Code of conduct

Please make sure ti abide by our [Code of Conduct](./CODE_OF_CONDUCT.md) while working on contributions.