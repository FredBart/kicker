# domain

# Team
- id?: Int
- name: String
- players: Player[]
- score?: Int
- history: Match[]

# Match
- id?: Int
- team1: Team
- team2: Team
- goals1: Int
- goals2: Int

# Player
- id?: Int
- name: String
- teams: Team[]