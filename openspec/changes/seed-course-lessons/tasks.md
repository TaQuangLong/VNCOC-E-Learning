## 1. SeedLessonsAsync — Method Scaffold

- [ ] 1.1 Add private static `SeedLessonsAsync(AppDbContext db, CancellationToken ct)` method signature to `DatabaseSeeder`
- [ ] 1.2 Call `await SeedLessonsAsync(db, ct)` in `SeedAsync` immediately after `await SeedCoursesAsync(db, ct)`

## 2. Idempotency and Course Lookup

- [ ] 2.1 For each of the 15 course slugs, resolve the course `Id` via `FirstOrDefaultAsync(c => c.Slug == slug)` — skip if null
- [ ] 2.2 Before inserting, check `AnyAsync(l => l.CourseId == courseId)` — skip the course if lessons already exist

## 3. Lesson Data — Foundations of Spiritual Growth

- [ ] 3.1 Insert 6 lessons for slug `foundations-of-spiritual-growth`:
  - OrderIndex 1 (Preview): "Introduction to Spiritual Disciplines" — `EQ84qkq2Q6s` (399s)
  - OrderIndex 2: "The Role of Prayer in Growth" — `O0MyKgMzWBk` (5173s)
  - OrderIndex 3: "Reading Scripture with Understanding" — `4l85ZMZXaG8` (3656s)
  - OrderIndex 4: "Worship as a Way of Life" — `NJHh5SRtF7g` (2969s)
  - OrderIndex 5: "Finding Community in the Church" — `LpcXBQw1f_s` (3072s)
  - OrderIndex 6: "Living Out Your Faith Daily" — `pQ9CKq4xl9E` (2280s)

## 4. Lesson Data — Deep Roots: Advanced Spiritual Disciplines

- [ ] 4.1 Insert 6 lessons for slug `deep-roots-advanced-spiritual-disciplines`:
  - OrderIndex 1 (Preview): "Fasting and Solitude" — `Saw84fmOBBs` (3196s)
  - OrderIndex 2: "Contemplative Prayer" — `O0MyKgMzWBk` (5173s)
  - OrderIndex 3: "Spiritual Direction and Accountability" — `CtjdTNuzTzc` (3392s)
  - OrderIndex 4: "Sabbath and Rest as Discipline" — `yaf4uGpe3gI` (3007s)
  - OrderIndex 5: "Service and Humility" — `DTDdX3F7rao` (2366s)
  - OrderIndex 6: "Spiritual Milestones and Ebenezer Stones" — `4l85ZMZXaG8` (3656s)

## 5. Lesson Data — Core Christian Doctrines

- [ ] 5.1 Insert 6 lessons for slug `core-christian-doctrines`:
  - OrderIndex 1 (Preview): "The Trinity Explained" — `NJHh5SRtF7g` (2969s)
  - OrderIndex 2: "The Nature of Christ" — `LpcXBQw1f_s` (3072s)
  - OrderIndex 3: "The Doctrine of Salvation" — `Saw84fmOBBs` (3196s)
  - OrderIndex 4: "The Authority of Scripture" — `4l85ZMZXaG8` (3656s)
  - OrderIndex 5: "The Church and Its Purpose" — `pQ9CKq4xl9E` (2280s)
  - OrderIndex 6: "The Final Hope — Eschatology Basics" — `DTDdX3F7rao` (2366s)

## 6. Lesson Data — Systematic Theology Essentials

- [ ] 6.1 Insert 6 lessons for slug `systematic-theology-essentials`:
  - OrderIndex 1 (Preview): "Bibliology — The Word of God" — `4l85ZMZXaG8` (3656s)
  - OrderIndex 2: "Theology Proper — Who Is God?" — `NJHh5SRtF7g` (2969s)
  - OrderIndex 3: "Christology — The Person of Christ" — `LpcXBQw1f_s` (3072s)
  - OrderIndex 4: "Soteriology — The Doctrine of Salvation" — `CtjdTNuzTzc` (3392s)
  - OrderIndex 5: "Ecclesiology — The Doctrine of the Church" — `pQ9CKq4xl9E` (2280s)
  - OrderIndex 6: "Eschatology — Last Things" — `DTDdX3F7rao` (2366s)

## 7. Lesson Data — The Early Church 100–500 AD

- [ ] 7.1 Insert 6 lessons for slug `the-early-church-100-500-ad`:
  - OrderIndex 1 (Preview): "The Apostolic Fathers" — `FOgkX2wqGuQ` (179s)
  - OrderIndex 2: "The First Persecutions" — `rbxx65C2f_U` (236s)
  - OrderIndex 3: "Defining Orthodoxy — The Councils" — `EQ84qkq2Q6s` (399s)
  - OrderIndex 4: "Ignatius, Polycarp, and the Martyrs" — `O0MyKgMzWBk` (5173s)
  - OrderIndex 5: "Athanasius and the Arian Controversy" — `Saw84fmOBBs` (3196s)
  - OrderIndex 6: "Augustine and Christian Thought" — `4l85ZMZXaG8` (3656s)

## 8. Lesson Data — Reformation & Modern Christianity

- [ ] 8.1 Insert 6 lessons for slug `reformation-and-modern-christianity`:
  - OrderIndex 1 (Preview): "Luther and the 95 Theses" — `EQ84qkq2Q6s` (399s)
  - OrderIndex 2: "Calvin and Reformed Theology" — `DTDdX3F7rao` (2366s)
  - OrderIndex 3: "The Counter-Reformation" — `pQ9CKq4xl9E` (2280s)
  - OrderIndex 4: "The Great Awakenings" — `CtjdTNuzTzc` (3392s)
  - OrderIndex 5: "Missions and the Modern Church" — `LpcXBQw1f_s` (3072s)
  - OrderIndex 6: "Christianity in the 21st Century" — `yaf4uGpe3gI` (3007s)

## 9. Lesson Data — Building a Christ-Centered Marriage

- [ ] 9.1 Insert 6 lessons for slug `building-a-christ-centered-marriage`:
  - OrderIndex 1 (Preview): "God's Design for Marriage" — `LpcXBQw1f_s` (3072s)
  - OrderIndex 2: "Communication and Listening Well" — `yaf4uGpe3gI` (3007s)
  - OrderIndex 3: "Conflict Resolution in Marriage" — `O0MyKgMzWBk` (5173s)
  - OrderIndex 4: "Building Spiritual Intimacy Together" — `NJHh5SRtF7g` (2969s)
  - OrderIndex 5: "Forgiveness and Grace in Marriage" — `Saw84fmOBBs` (3196s)
  - OrderIndex 6: "Praying Together as a Couple" — `pQ9CKq4xl9E` (2280s)

## 10. Lesson Data — Healthy Relationships in Community

- [ ] 10.1 Insert 6 lessons for slug `healthy-relationships-in-community`:
  - OrderIndex 1 (Preview): "What the Bible Says About Community" — `LpcXBQw1f_s` (3072s)
  - OrderIndex 2: "The Art of Listening and Empathy" — `yaf4uGpe3gI` (3007s)
  - OrderIndex 3: "Accountability and Genuine Friendship" — `CtjdTNuzTzc` (3392s)
  - OrderIndex 4: "Forgiveness — Releasing the Debt" — `Saw84fmOBBs` (3196s)
  - OrderIndex 5: "Navigating Conflict Biblically" — `DTDdX3F7rao` (2366s)
  - OrderIndex 6: "Building Lasting Bonds of Trust" — `4l85ZMZXaG8` (3656s)

## 11. Lesson Data — Servant Leadership in the Church

- [ ] 11.1 Insert 6 lessons for slug `servant-leadership-in-the-church`:
  - OrderIndex 1 (Preview): "Jesus the Servant Leader" — `LpcXBQw1f_s` (3072s)
  - OrderIndex 2: "Discovering Your Spiritual Gifts" — `NJHh5SRtF7g` (2969s)
  - OrderIndex 3: "Leading with Humility" — `4l85ZMZXaG8` (3656s)
  - OrderIndex 4: "Serving Without Burning Out" — `CtjdTNuzTzc` (3392s)
  - OrderIndex 5: "Integrity and Character in Leadership" — `yaf4uGpe3gI` (3007s)
  - OrderIndex 6: "Developing the Next Generation of Leaders" — `pQ9CKq4xl9E` (2280s)

## 12. Lesson Data — Growing Your Ministry Team

- [ ] 12.1 Insert 6 lessons for slug `growing-your-ministry-team`:
  - OrderIndex 1 (Preview): "Vision Casting for Your Ministry" — `EQ84qkq2Q6s` (399s)
  - OrderIndex 2: "Identifying and Recruiting Leaders" — `rbxx65C2f_U` (236s)
  - OrderIndex 3: "Delegation and Empowerment" — `O0MyKgMzWBk` (5173s)
  - OrderIndex 4: "Building a Team Culture of Grace" — `Saw84fmOBBs` (3196s)
  - OrderIndex 5: "Discipleship Pipelines That Work" — `DTDdX3F7rao` (2366s)
  - OrderIndex 6: "Evaluating and Growing Your Ministry" — `4l85ZMZXaG8` (3656s)

## 13. Lesson Data — Walking Through the Old Testament

- [ ] 13.1 Insert 6 lessons for slug `walking-through-the-old-testament`:
  - OrderIndex 1 (Preview): "Creation, Fall, and the Promise" — `EQ84qkq2Q6s` (399s)
  - OrderIndex 2: "Abraham and the Covenant" — `rbxx65C2f_U` (236s)
  - OrderIndex 3: "Moses, the Law, and the Exodus" — `O0MyKgMzWBk` (5173s)
  - OrderIndex 4: "The Psalms — Songs of the Heart" — `NJHh5SRtF7g` (2969s)
  - OrderIndex 5: "The Prophets — God's Voice to His People" — `DTDdX3F7rao` (2366s)
  - OrderIndex 6: "How the Old Testament Points to Christ" — `4l85ZMZXaG8` (3656s)

## 14. Lesson Data — Life of Christ: The Four Gospels

- [ ] 14.1 Insert 6 lessons for slug `life-of-christ-the-four-gospels`:
  - OrderIndex 1 (Preview): "The Birth and Early Life of Jesus" — `FOgkX2wqGuQ` (179s)
  - OrderIndex 2: "The Baptism and Temptation of Jesus" — `rbxx65C2f_U` (236s)
  - OrderIndex 3: "The Teachings and Parables of Jesus" — `LpcXBQw1f_s` (3072s)
  - OrderIndex 4: "Miracles and the Kingdom of God" — `4l85ZMZXaG8` (3656s)
  - OrderIndex 5: "The Passion, Death, and Resurrection" — `Saw84fmOBBs` (3196s)
  - OrderIndex 6: "The Great Commission — Go and Make Disciples" — `pQ9CKq4xl9E` (2280s)

## 15. Lesson Data — The Art of Prayer

- [ ] 15.1 Insert 6 lessons for slug `the-art-of-prayer`:
  - OrderIndex 1 (Preview): "What Is Prayer and Why It Matters" — `EQ84qkq2Q6s` (399s)
  - OrderIndex 2: "The Lord's Prayer — A Model to Follow" — `O0MyKgMzWBk` (5173s)
  - OrderIndex 3: "Intercession — Praying for Others" — `CtjdTNuzTzc` (3392s)
  - OrderIndex 4: "Listening Prayer and Silence" — `yaf4uGpe3gI` (3007s)
  - OrderIndex 5: "Prayer and Fasting Together" — `Saw84fmOBBs` (3196s)
  - OrderIndex 6: "Leading Corporate Prayer and Worship" — `NJHh5SRtF7g` (2969s)

## 16. Lesson Data — Sharing Your Faith

- [ ] 16.1 Insert 6 lessons for slug `sharing-your-faith`:
  - OrderIndex 1 (Preview): "Why Every Believer Is Called to Share" — `FOgkX2wqGuQ` (179s)
  - OrderIndex 2: "Overcoming Fear in Evangelism" — `rbxx65C2f_U` (236s)
  - OrderIndex 3: "Your Personal Testimony — Telling Your Story" — `EQ84qkq2Q6s` (399s)
  - OrderIndex 4: "Everyday Evangelism — Natural Conversations" — `LpcXBQw1f_s` (3072s)
  - OrderIndex 5: "Apologetics Basics — Answering Hard Questions" — `CtjdTNuzTzc` (3392s)
  - OrderIndex 6: "Cross-Cultural Sensitivity in Ministry" — `pQ9CKq4xl9E` (2280s)

## 17. Lesson Data — Raising Faith-Filled Families

- [ ] 17.1 Insert 6 lessons for slug `raising-faith-filled-families`:
  - OrderIndex 1 (Preview): "The Parent as Discipler" — `FOgkX2wqGuQ` (179s)
  - OrderIndex 2: "Family Devotions That Actually Work" — `yaf4uGpe3gI` (3007s)
  - OrderIndex 3: "Talking About Faith at Every Stage" — `O0MyKgMzWBk` (5173s)
  - OrderIndex 4: "Modeling the Christian Life for Kids" — `LpcXBQw1f_s` (3072s)
  - OrderIndex 5: "Navigating Doubts with Your Children" — `Saw84fmOBBs` (3196s)
  - OrderIndex 6: "Passing Down a Heritage of Faith" — `NJHh5SRtF7g` (2969s)

## 18. Verification

- [ ] 18.1 Run app locally with a fresh database and confirm 90 lessons appear across all 15 courses
- [ ] 18.2 Run app a second time and confirm no duplicate lessons are inserted (idempotency)
- [ ] 18.3 Verify each course's lesson 1 has `IsPreview = true` in the database
