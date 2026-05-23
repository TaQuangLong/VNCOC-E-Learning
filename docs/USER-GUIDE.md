# ChurchLearn — User Guide

## Table of Contents

1. [Roles Overview](#1-roles-overview)
2. [Admin Guide — Setting Up Courses](#2-admin-guide--setting-up-courses)
   - [Access the Admin Panel](#21-access-the-admin-panel)
   - [Create a Course](#22-create-a-course)
   - [Add Lessons to a Course](#23-add-lessons-to-a-course)
   - [Add a Quiz to a Lesson](#24-add-a-quiz-to-a-lesson)
   - [Publish a Course](#25-publish-a-course)
   - [Manage Learners & Progress](#26-manage-learners--progress)
   - [Assign or Change User Roles](#27-assign-or-change-user-roles)
   - [Admin Dashboard & Reports](#28-admin-dashboard--reports)
3. [Student Guide — Register & Learn](#3-student-guide--register--learn)
   - [Create an Account](#31-create-an-account)
   - [Log In & Reset Password](#32-log-in--reset-password)
   - [Browse & Enroll in a Course](#33-browse--enroll-in-a-course)
   - [Learn a Lesson](#34-learn-a-lesson)
   - [Take a Quiz](#35-take-a-quiz)
   - [Join Discussions](#36-join-discussions)
   - [Track Your Progress](#37-track-your-progress)

---

## 1. Roles Overview

| Role | Who | What They Can Do |
|------|-----|------------------|
| **Student** | Every registered member | Browse courses, enroll, watch/read lessons, take quizzes, join discussions, view own progress |
| **Admin** | Course managers / staff | Everything a Student can do, plus: create/edit/delete courses and lessons, manage quizzes, view all learner progress |
| **SuperAdmin** | Platform owner | Everything an Admin can do, plus: assign or change roles for any user |

> New registrations are automatically assigned the **Student** role.  
> A SuperAdmin must manually promote a user to Admin or SuperAdmin via the admin panel.

---

## 2. Admin Guide — Setting Up Courses

### 2.1 Access the Admin Panel

1. Log in with an **Admin** or **SuperAdmin** account.
2. Navigate to `/admin/courses` (e.g. `https://yoursite.com/admin/courses`).
3. You will see the list of all courses and a **New Course** button.

> If the menu is not visible, your account may not have the Admin role. Contact a SuperAdmin.

---

### 2.2 Create a Course

1. Go to **Admin → Courses** and click **New Course** (`/admin/courses/new`).
2. Fill in the course details:

| Field | Required | Notes |
|-------|----------|-------|
| **Title** | Yes | Max 200 characters |
| **Slug** | Yes | URL-friendly ID — lowercase kebab-case only, e.g. `intro-to-faith`. Must be unique. |
| **Short Description** | No | Shown on the course card. Max 500 characters. |
| **Description** | No | Full course overview displayed on the detail page. |
| **Thumbnail URL** | No | Must be a valid `https://` URL pointing to an image. |
| **Category** | No | Free-text category label (e.g. "Bible Study", "Worship") |
| **Level** | No | Choose: `Beginner`, `Intermediate`, or `Advanced` |
| **Language** | No | Language of the course content (e.g. "English", "Vietnamese") |
| **Author** | Yes | Select from the registered authors list |

3. Click **Save**. The course is created with a **Draft** status and is not visible to students yet.

---

### 2.3 Add Lessons to a Course

1. From **Admin → Courses**, click the **Lessons** icon next to the course (`/admin/courses/:courseId/lessons`).
2. Click **New Lesson** (`/admin/courses/:courseId/lessons/new`).
3. Fill in the lesson details:

| Field | Required | Notes |
|-------|----------|-------|
| **Title** | Yes | Lesson name shown in the sidebar |
| **Description** | No | Optional summary of what this lesson covers |
| **Content Type** | Yes | Choose one: `Video`, `Text`, or `PDF` |
| **YouTube URL** | If Video | Paste the full YouTube video URL |
| **Text Content** | If Text | Rich text content for the lesson body |
| **PDF URL** | If PDF | Must be a valid `https://` URL to a PDF file |
| **Duration (seconds)** | Yes | Estimated lesson length — shown to students |
| **Order Index** | Yes | Controls lesson order in the sidebar (1 = first) |
| **Is Preview** | No | If checked, non-enrolled visitors can view this lesson |

4. Click **Save**. Repeat for each lesson.

> **Tip:** Use **Order Index** to reorder lessons. Lower numbers appear first.

---

### 2.4 Add a Quiz to a Lesson

Quizzes are attached to individual lessons.

1. Open the lesson in edit mode (`/admin/courses/:courseId/lessons/:lessonId/edit`).
2. Scroll to the **Quiz** section and click **Add Quiz**.
3. Fill in the quiz settings:

| Field | Required | Notes |
|-------|----------|-------|
| **Title** | Yes | Quiz name, max 200 characters |
| **Description** | No | Optional instructions shown before the quiz starts |
| **Passing Score** | Yes | Percentage required to pass (0–100) |
| **Is Required** | No | If checked, the student must pass before progressing |

4. Add at least **one question**:
   - Enter the question text (max 1,000 characters).
   - Choose the question type (e.g. Multiple Choice).
   - Add at least **two answer options**.
   - Mark the correct answer(s).
   - Set the order index.

5. Click **Save Quiz**.

---

### 2.5 Publish a Course

A course must be **Published** before students can see and enroll in it.

1. Go to **Admin → Courses** and click **Edit** on the course.
2. Scroll to the **Status** field and change it from `Draft` to `Published`.
3. Click **Save**.

> You can set it back to `Draft` at any time to hide it from students.

---

### 2.6 Manage Learners & Progress

**View enrolled learners:**

1. Go to **Admin → Courses** and click the **Learners** icon (`/admin/courses/:courseId/learners`).
2. You will see a list of all enrolled users with their enrollment date and progress percentage.

**View a specific user's progress:**

1. Click on a learner's name in the learners list, or go to `/admin/users/:userId/progress`.
2. You will see a lesson-by-lesson breakdown of what the user has completed.

---

### 2.7 Assign or Change User Roles

> Only a **SuperAdmin** can perform this action.

1. Go to **Admin → Users** (accessible via the API or a future admin UI page).
2. Locate the user by their ID or email.
3. Update their roles — valid values are: `Student`, `Admin`, `SuperAdmin`.

> A user can hold multiple roles simultaneously.

---

### 2.8 Admin Dashboard & Reports

Navigate to `/admin` to view the overview dashboard, which shows:

- **Total registered users** and new users in the last 7 days
- **Total published courses**
- **Total active enrollments**
- **Total quiz attempts**
- **Top 5 most popular courses** by enrollment count
- **10 most recently registered users**

---

## 3. Student Guide — Register & Learn

### 3.1 Create an Account

1. Open the site in your browser — you will land on the **Courses** page (`/courses`).
2. Click **Register** or go to `/register`.
3. Fill in:
   - **Display Name** — your name as it appears to others
   - **Email** — must be a valid address
   - **Password** — follow the strength requirements shown on screen
4. Click **Register**. You are automatically logged in as a **Student**.

---

### 3.2 Log In & Reset Password

**Log in:**

1. Go to `/login`.
2. Enter your **Email** and **Password**, then click **Log In**.

**Forgot your password:**

1. On the login page, click **Forgot Password**.
2. Enter your email address and click **Send Reset Link**.
3. Check your inbox for a password reset email.
4. Click the link in the email and enter a new password on `/reset-password`.

---

### 3.3 Browse & Enroll in a Course

1. Go to `/courses` to see all published courses.
2. Click on a course card to open the **Course Detail** page (`/courses/:slug`).
3. Review the course description, level, language, and lesson list.
4. Click **Enroll** to join the course. You must be logged in.

> Some lessons may be marked as **Preview** — you can view those without enrolling.

---

### 3.4 Learn a Lesson

1. After enrolling, click **Start Learning** on the course detail page, or go to **My Learning** (`/my-learning`) and click on the course.
2. You are taken to the **Learn Page** (`/learn/:courseId/lessons/:lessonId`), which shows:
   - A **sidebar** listing all lessons in order
   - The **lesson content** (video player, text, or PDF)
   - A **discussion thread** for the lesson (see below)
3. Lessons are marked as **Complete** automatically when you finish viewing or reading them.
4. Use the **Next** / **Previous** buttons to navigate between lessons.

---

### 3.5 Take a Quiz

If a lesson has a quiz attached:

1. After the lesson content, you will see a **Take Quiz** button.
2. Read each question and select your answer(s).
3. Click **Submit**.
4. Your score is shown immediately. If you meet the **Passing Score**, the quiz is marked as passed.
5. You can review your past attempts in the quiz results section.

> If the quiz is marked **Required**, you must pass it to unlock the next lesson.

---

### 3.6 Join Discussions

Each lesson has a **Discussion** section below the content.

1. Scroll down on the Learn Page to the **Discussion** panel.
2. Click **Start a Discussion** or **Add a Comment** to post a new message.
3. Click **Reply** on any existing post to respond.
4. You can edit or delete your own posts at any time.

> Admins can remove any post that violates community guidelines.

---

### 3.7 Track Your Progress

**My Learning page (`/my-learning`):**

- Shows all courses you are enrolled in.
- Displays a **progress bar** for each course (percentage of lessons completed).
- Click a course to continue where you left off.

**Per-lesson progress:**

- Completed lessons are marked with a checkmark in the lesson sidebar.
- Your overall course completion percentage updates in real time as you finish lessons.
