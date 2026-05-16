# ChurchLearn — Feature Build Order and Dependencies

_Update status column each sprint._

---

## Dependency Tree

```
Foundation (no deps)
  └── Auth (needs: User, Role)
        └── Courses Backend (needs: Auth, Author entity)
              └── Lessons Backend (needs: Courses)
                    └── Learning Page Frontend (needs: Lessons API)
                          └── Enrollment (needs: Auth + Courses)
                                └── Progress (needs: Enrollment + Lessons)
                                      └── Quiz (needs: Lessons + Progress)
                                            └── Discussion (needs: Lessons + Auth)
                                                  └── Reports (needs: all above)
```

---

## Sprint to Feature Map

| Sprint | Feature                | Status      | Entities Added                              | Depends On            |
|--------|------------------------|-------------|---------------------------------------------|-----------------------|
| 0      | Project Setup          | done        | —                                           | None                  |
| 1      | Foundation             | in-progress | —                                           | None                  |
| 2      | Auth                   | not-started | User, Role                                  | Foundation            |
| 3      | Courses Backend        | not-started | Course, Author                              | Auth                  |
| 4      | Courses Frontend       | not-started | —                                           | Courses Backend       |
| 5      | Lessons Backend        | not-started | Lesson, Resource                            | Courses Backend       |
| 6      | Learning Page Frontend | not-started | —                                           | Lessons Backend       |
| 7      | Enrollment             | not-started | Enrollment                                  | Auth, Courses         |
| 8      | Progress               | not-started | LessonProgress                              | Enrollment, Lessons   |
| 9      | Quiz Backend           | not-started | Quiz, Question, AnswerOption, QuizAttempt   | Lessons, Progress     |
| 10     | Quiz Frontend          | not-started | —                                           | Quiz Backend          |
| 11     | Discussion Backend     | not-started | Discussion                                  | Lessons, Auth         |
| 12     | Discussion Frontend    | not-started | —                                           | Discussion Backend    |
| 13     | Reports                | not-started | —                                           | All core features     |
| 14     | CI/CD                  | not-started | —                                           | All features          |
| 15     | Production             | not-started | —                                           | CI/CD                 |
| 16     | Pilot Launch           | not-started | —                                           | Production            |
