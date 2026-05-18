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

| Sprint | Feature                | Status      | Entities Added                              | Depends On            | Spec                                        |
|--------|------------------------|-------------|---------------------------------------------|-----------------------|---------------------------------------------|
| 0      | Project Setup          | ✅ done     | —                                           | None                  | specs/sprint-00-preparation/SPEC.md         |
| 1      | Foundation             | ✅ done     | —                                           | None                  | specs/sprint-01-setup/SPEC.md               |
| 2      | Auth                   | ✅ done     | AppUser, Role                                | Foundation            | specs/sprint-02-auth/SPEC.md                |
| 3      | Courses Backend        | ✅ done        | Course, Author                           | Auth                  | specs/sprint-03-courses-backend/SPEC.md     |
| 4      | Courses Frontend       | ✅ done        | —                                        | Courses Backend       | specs/sprint-04-courses-frontend/SPEC.md    |
| 5      | Lessons Backend        | ✅ done        | Lesson, Resource                         | Courses Backend       | specs/sprint-05-lessons-backend/SPEC.md     |
| 6      | Learning Page Frontend | ✅ done        | —                                        | Lessons Backend       | specs/sprint-06-learning-page/SPEC.md       |
| 7      | Enrollment             | ✅ done        | Enrollment                               | Auth, Courses         | specs/sprint-07-enrollment/SPEC.md          |
| 8      | Progress               | ✅ done        | LessonProgress                           | Enrollment, Lessons   | specs/sprint-08-progress/SPEC.md            |
| 9      | Quiz Backend           | 🔲 not-started | Quiz, Question, AnswerOption, QuizAttempt | Lessons, Progress    | specs/sprint-09-quiz-backend/SPEC.md        |
| 10     | Quiz Frontend          | 🔲 not-started | —                                        | Quiz Backend          | specs/sprint-10-quiz-frontend/SPEC.md       |
| 11     | Discussion Backend     | 🔲 not-started | Discussion                               | Lessons, Auth         | specs/sprint-11-discussion-backend/SPEC.md  |
| 12     | Discussion Frontend    | 🔲 not-started | —                                        | Discussion Backend    | specs/sprint-12-discussion-frontend/SPEC.md |
| 13     | Reports                | 🔲 not-started | —                                        | All core features     | specs/sprint-13-reports/SPEC.md             |
| 14     | CI/CD                  | 🔲 not-started | —                                        | All features          | specs/sprint-14-cicd/SPEC.md                |
| 15     | Production             | 🔲 not-started | —                                        | CI/CD                 | specs/sprint-15-production/SPEC.md          |
| 16     | Pilot Launch           | 🔲 not-started | —                                        | Production            | specs/sprint-16-pilot/SPEC.md               |
